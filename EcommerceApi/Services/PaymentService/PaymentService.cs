using EcommerceApi.Config;
using EcommerceApi.Constant;
using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Lib;
using EcommerceApi.Models;
using EcommerceApi.Models.Order;
using EcommerceApi.Models.Payment;
using EcommerceApi.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace EcommerceApi.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly VnPayConfig _config;
        private readonly EcommerceDbContext _context;
        public PaymentService(EcommerceDbContext context, IOptions<VnPayConfig> options) {
            _config = options.Value;
            _context = context;
        }

        public async Task<InvoiceResponse> PostPaymentReturnAsync(TranDto tranDto, HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            var invoiceResponse = new InvoiceResponse();
            var newTransaction = new PaymentTransaction();
            var tranId = tranDto.TranId;

            PaymentTransaction? currTran = null;
            if(!string.IsNullOrEmpty(tranId))
            {
                currTran = await _context
                                          .PaymentTransactions
                                          .Where(pt => pt.TranscationId.ToString().ToLower().Equals(tranId))
                                          .FirstOrDefaultAsync(cancellationToken);
            }

            if (httpRequest.Query.Count > 0)
            {
                string vnp_HashSecret = _config.vnp_HashSecret;//Chuoi bi mat
                var vnpayData = httpRequest.Query;
                VnPayLibrary vnpay = new();

                foreach (KeyValuePair<string, StringValues> s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s.Value) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value!);
                    }
                }
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_newTransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                string orderId = vnpay.GetResponseData("vnp_TxnRef");
                string vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = httpRequest.Query["vnp_SecureHash"]!;
                int vnp_Amount = Convert.ToInt32(vnpay.GetResponseData("vnp_Amount")) / 100;
                Payment payment = new();
                Order order = new();
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                            order = await _context
                                                  .Orders
                                                  .Where(o => o.OrderId.ToString().ToLower().Equals(orderId.ToString().ToLower()))
                                                  .Include(o => o.OrderDetails)
                                                  .ThenInclude(od => od.Product)
                                                  .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Order not found.");
                            payment = await _context
                                                    .Payments
                                                    .Where(p => p.PaymentOrderId.ToString().ToLower().Equals(orderId.ToString().ToLower()))
                                                    .FirstOrDefaultAsync(cancellationToken)
                                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Payment not found.");
                        //thanh toan thanh cong
                        invoiceResponse.Message = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        invoiceResponse.OrderId = Guid.Parse(orderId);
                        invoiceResponse.Amount = vnp_Amount;
                        invoiceResponse.TranNo = vnpayTranId;
                        invoiceResponse.CustomerId = order.UserId;
                        invoiceResponse.CustomerEmail = order.Email;
                        invoiceResponse.CustomerName = order.FullName;
                        invoiceResponse.CustomerPhone = order.PhoneNumber;
                        invoiceResponse.InvoiceAddress = order.DeliveryAddress;
                        invoiceResponse.InvoiceDate = order.OrderDate;
                        invoiceResponse.OrderDetails = order.OrderDetails;
                        if (currTran is null)
                        {
                            //tao transac
                            newTransaction.TranscationId = Guid.NewGuid();
                            newTransaction.TranPayload = vnpayTranId;
                            newTransaction.TranStatus = vnp_TransactionStatus;
                            newTransaction.TranMessage = invoiceResponse.Message;
                            newTransaction.TranAmount = vnp_Amount;
                            newTransaction.CreatedAt = DateTime.Now;
                            newTransaction.PaymentId = payment.PaymentId;

                            //cap nhat payment message
                            payment.PaidAmout = vnp_Amount;
                            payment.PaymentStatus = PaymentStatus.Succeed;
                            payment.PaymentLastMessage = "Order payment successful";
                            invoiceResponse.TranId = newTransaction?.TranscationId;

                            //update trang thai order
                            order.Status = PaymentStatus.Succeed;
                        }
                        else
                        {
                            invoiceResponse.TranId = currTran?.TranscationId;
                        }
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        var errorMessage = "An error occurred during processing. Error code: " + vnp_ResponseCode;
                        if(currTran is null)
                        {
                            payment.PaymentLastMessage = errorMessage;
                            payment.PaymentStatus = PaymentStatus.Failed;
                            order.Status = PaymentStatus.Failed;
                            newTransaction.TranscationId = Guid.NewGuid();
                            newTransaction.TranPayload = vnpayTranId;
                            newTransaction.TranStatus = vnp_TransactionStatus;
                            newTransaction.TranMessage = errorMessage;
                            newTransaction.TranAmount = vnp_Amount;
                            newTransaction.CreatedAt = DateTime.Now;
                            newTransaction.PaymentId = payment.PaymentId;
                            invoiceResponse.TranId = newTransaction?.TranscationId;
                        }
                        else
                        {
                            invoiceResponse.TranId = currTran?.TranscationId;
                        }
                        throw new HttpStatusException(HttpStatusCode.InternalServerError, errorMessage);
                    }
                }
                else
                {
                    var errorMessage = "An error occurred during processing.";
                    if (currTran is null)
                    {
                        payment.PaymentLastMessage = errorMessage;
                        payment.PaymentStatus = PaymentStatus.Failed;
                        order.Status = PaymentStatus.Failed;
                        newTransaction.TranscationId = Guid.NewGuid();
                        newTransaction.TranPayload = vnpayTranId;
                        newTransaction.TranStatus = vnp_TransactionStatus;
                        newTransaction.TranMessage = errorMessage;
                        newTransaction.TranAmount = vnp_Amount;
                        newTransaction.CreatedAt = DateTime.Now;
                        newTransaction.PaymentId = payment.PaymentId;
                        invoiceResponse.TranId = newTransaction?.TranscationId;
                    }
                    else
                    {
                        invoiceResponse.TranId = currTran?.TranscationId;
                    }
                    throw new HttpStatusException(HttpStatusCode.InternalServerError, errorMessage);
                }
            }
            if(currTran is null)
            {
                await _context.PaymentTransactions.AddAsync(newTransaction, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return invoiceResponse;
        }

        public async Task<PaymentResponse> PostPaymentOrderAsync(PaymentDto paymentDto,HttpRequest httpRequest ,CancellationToken cancellationToken)
        {
            var merchantOfCurrentPayment = await _context
                                                         .Merchants
                                                         .Where(m => m.MerchantId == paymentDto.MerchantId)
                                                         .FirstOrDefaultAsync(cancellationToken)
                                                         ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Merchant not found.");
            var destinationOfCurrentPayment = await _context
                                                            .PaymentDestinations
                                                            .Where(pd => pd.DestinationId == paymentDto.DestinationId)
                                                            .FirstOrDefaultAsync(cancellationToken)
                                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Destinate not found.");
            using (var newTransaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    var newOrder = new Order()
                    {
                        OrderId = Guid.NewGuid(),
                        OrderDate = DateTime.Now,
                        Status = "pending",
                        Email = paymentDto.Email,
                        CouponId = paymentDto?.CouponId,
                        UserId = paymentDto.UserId,
                        TotalDiscount = paymentDto.TotalDiscount,
                        TotalPrice = Convert.ToDecimal(paymentDto.RequiredAmount),
                        TotalQuantity = paymentDto.TotalQuantity,
                        Note = paymentDto.Note,
                        DeliveryAddress = $"{paymentDto.Address.Trim()}, {paymentDto.Ward.Trim()}, {paymentDto.District.Trim()}, {paymentDto.City.Trim()}, {paymentDto.Country.Trim()}",
                        PhoneNumber = paymentDto.Phone,
                        FullName = paymentDto.FullName,
                    };
                    await _context.Orders.AddAsync(newOrder, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    var listOrderDetail = new List<OrderDetail>();
                    foreach(var od in paymentDto.OrderDetails) {
                        var newOrderDetail = new OrderDetail()
                        {
                            OrderId = newOrder.OrderId,
                            ProductId = od.ProductId,
                            PriceProduct = od.PriceProduct,
                            QuantityProduct = od.QuantityProduct,
                            DiscountProduct = od.DiscountProduct,
                        };
                        listOrderDetail.Add(newOrderDetail);
                    }

                    var newPayment = new Payment()
                    {
                        PaymentId = Guid.NewGuid(),
                        PaymentContent = "Thanh toan don hang:" + newOrder.OrderId.ToString(),
                        PaymentCurrency = "VND",
                        RequiredAmount = paymentDto.RequiredAmount,
                        PaymentLanguage = "vn",
                        PaymentOrderId = newOrder.OrderId,
                        MerchantId = paymentDto.MerchantId,
                        PaymentDestinationId = paymentDto.DestinationId,
                        CreatedAt = DateTime.Now,
                        ExpiredAt = DateTime.Now.AddMinutes(15),
                        PaymentStatus = "pending"
                    };

                    var newSign = new PaymentSignature()
                    {
                        SignatureId = Guid.NewGuid(),
                        SignValue = Utils.HmacSHA512(_config.vnp_HashSecret, newPayment.PaymentId.ToString()),
                        SignAlgorithm = "HmacSHA512",
                        CreatedAt = DateTime.Now,
                        Payment = newPayment,
                        PaymentId = newPayment.PaymentId,
                        SignOwn = paymentDto.MerchantId.ToString(),
                    };

                    VnPayLibrary vnpay = new();

                    vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                    vnpay.AddRequestData("vnp_Command", "pay");
                    vnpay.AddRequestData("vnp_BankCode", "NCB");
                    vnpay.AddRequestData("vnp_TmnCode", _config.vnp_TmnCode);
                    vnpay.AddRequestData("vnp_Amount", (paymentDto.RequiredAmount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
                    vnpay.AddRequestData("vnp_CreateDate", newPayment.CreatedAt.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", httpRequest.HttpContext.Connection.RemoteIpAddress!.ToString());
                    //if (!string.IsNullOrEmpty(locale))
                    //{
                    //    vnpay.AddRequestData("vnp_Locale", locale);
                    //}
                    //else
                    //{
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    //}
                    vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + newOrder.OrderId.ToString());
                    vnpay.AddRequestData("vnp_OrderType", "130000"); //default value: other
                    vnpay.AddRequestData("vnp_ReturnUrl", _config.vnp_Returnurl);
                    vnpay.AddRequestData("vnp_TxnRef", newOrder.OrderId.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày
                                                                                   //Add Params of 2.1.0 Version
                    vnpay.AddRequestData("vnp_ExpireDate", newPayment.ExpiredAt.ToString("yyyyMMddHHmmss"));
                    //Billing
                    vnpay.AddRequestData("vnp_Bill_Email", paymentDto.Email.Trim());
                    var fullName = paymentDto.FullName.Trim();
                    if (!String.IsNullOrEmpty(fullName))
                    {
                        var indexof = fullName.IndexOf(' ');
                        vnpay.AddRequestData("vnp_Bill_FirstName", fullName.Substring(0, indexof));
                        vnpay.AddRequestData("vnp_Bill_LastName", fullName.Substring(indexof + 1, fullName.Length - indexof - 1));
                    }
                    //vnpay.AddRequestData("vnp_Bill_Address", paymentDto.Address.Trim());
                    //vnpay.AddRequestData("vnp_Bill_City", paymentDto.Ward.Trim());
                    //vnpay.AddRequestData("vnp_Bill_Country", paymentDto.City.Trim());
                    //vnpay.AddRequestData("vnp_Bill_State", "");
                    //// Invoice
                    vnpay.AddRequestData("vnp_Inv_Phone", paymentDto.Phone.Trim());
                    vnpay.AddRequestData("vnp_Inv_Email", paymentDto.Email.Trim());
                    vnpay.AddRequestData("vnp_Inv_Customer", paymentDto.FullName.Trim());
                    vnpay.AddRequestData("vnp_Inv_Address", $"{paymentDto.Address.Trim()}, {paymentDto.Ward.Trim()}, {paymentDto.District.Trim()}, {paymentDto.City.Trim()}, {paymentDto.Country.Trim()}");
                    //vnpay.AddRequestData("vnp_Inv_Company", txt_inv_company.Text);
                    //vnpay.AddRequestData("vnp_Inv_Taxcode", txt_inv_taxcode.Text);
                    //vnpay.AddRequestData("vnp_Inv_Type", cbo_inv_type.SelectedItem.Value);

                    string paymentUrl = vnpay.CreateRequestUrl(_config.vnp_Url, _config.vnp_HashSecret);

                    newPayment.PaymentUrl = paymentUrl;

                    await _context.OrderDetails.AddRangeAsync(listOrderDetail, cancellationToken);
                    await _context.Payments.AddAsync(newPayment, cancellationToken);
                    await _context.PaymentSignatures.AddAsync(newSign, cancellationToken);

                    await _context.SaveChangesAsync(cancellationToken);            

                    await newTransaction.CommitAsync(cancellationToken);
                    
                    return new PaymentResponse()
                    {
                        PaymentUrl = paymentUrl,
                        PaymentId = newPayment.PaymentId
                    };
                }
                catch (Exception ex)
                {
                    throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        public async Task<IpnResponse> GetPaymentIpnAsync(HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            IpnResponse ipnResponse = new();
            if (httpRequest.Query.Count > 0)
            {
                string vnp_HashSecret = _config.vnp_HashSecret; //Secret key
                var vnpayData = httpRequest.Query;
                VnPayLibrary vnpay = new();
                foreach (var s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value!);
                    }
                }
                //Lay danh sach tham so tra ve tu VNPAY
                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                //vnp_SecureHash: HmacSHA512 cua du lieu tra ve

                var orderId = vnpay.GetResponseData("vnp_TxnRef");
                var vnp_Amount = Convert.ToDecimal(vnpay.GetResponseData("vnp_Amount")) / 100;
                var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = httpRequest.Query["vnp_SecureHash"]!;
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                Order? order = new();
                if (checkSignature)
                {
                    //Cap nhat ket qua GD
                    //Yeu cau: Truy van vao CSDL cua  Merchant => lay ra duoc OrderInfo
                    //Giả sử OrderInfo lấy ra được như giả lập bên dưới
                    order = await _context
                                          .Orders
                                          .Where(o => o.OrderId.ToString().ToLower().Equals(orderId.ToString().ToLower()))
                                          .Include(o => o.OrderDetails)
                                          .ThenInclude(od => od.Product)
                                          .FirstOrDefaultAsync(cancellationToken);//get from DB
                    //0: Cho thanh toan,1: da thanh toan,2: GD loi
                    //Kiem tra tinh trang Order
                    if (order != null)
                    {
                        if (order.TotalPrice == vnp_Amount)
                        {
                            if (order.Status == "pending")
                            {
                                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                                {
                                    //Thanh toan thanh cong
                                    // => thực hiện cập nhật trạng thái order
                                }
                                else
                                {
                                    //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                                    //  displayMsg.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                                    //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}",
                                    //    orderId,
                                    //    vnpayTranId, vnp_ResponseCode);
                                }

                                //Thêm code Thực hiện cập nhật vào Database 
                                //Update Database
                                ipnResponse.RspCode = "00";
                                ipnResponse.Message = "Confirm Success";
                            }
                            else
                            {
                                ipnResponse.RspCode = "02";
                                ipnResponse.Message = "Order already confirmed";
                            }
                        }
                        else
                        {
                            ipnResponse.RspCode = "04";
                            ipnResponse.Message = "invalid amount";                       
                        }
                    }
                    else
                    {
                        ipnResponse.RspCode = "01";
                        ipnResponse.Message = "Order not found";
                    }
                }
                else
                {

                    ipnResponse.RspCode = "97";
                    ipnResponse.Message = "Invalid signature";
                    //log.InfoFormat("Invalid signature, InputData={0}", Request.RawUrl);
                }
            }
            else
            {
                ipnResponse.RspCode = "99";
                ipnResponse.Message = "Input data required";
            }
            return ipnResponse;
        }
    }
}
