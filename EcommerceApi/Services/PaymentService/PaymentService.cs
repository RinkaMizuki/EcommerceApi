using EcommerceApi.Config;
using EcommerceApi.Constant;
using EcommerceApi.Dtos.User;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Hubs;
using EcommerceApi.Lib;
using EcommerceApi.Models;
using EcommerceApi.Models.Message;
using EcommerceApi.Models.Order;
using EcommerceApi.Models.Payment;
using EcommerceApi.Responses;
using EcommerceApi.Services.MailService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Net.payOS.Errors;
using Net.payOS.Types;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using EcommerceApi.Models.Product;

namespace EcommerceApi.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly VnPayConfig _config;
        private readonly EcommerceDbContext _context;
        private readonly IMailService _mailservice;
        private readonly IHubContext<OrderHub> _hubcontext;
        private readonly PayOsLibrary _payoslibrary;
        public PaymentService(EcommerceDbContext context, IMailService mailService, IOptions<VnPayConfig> options, PayOsLibrary payOsLibrary,IHubContext<OrderHub> hubContext) {
            _config = options.Value;
            _context = context;
            _mailservice = mailService;
            _payoslibrary = payOsLibrary;
            _hubcontext = hubContext;
        }

        public async Task<OrderResponse> PostPaymentVnPayReturnAsync(HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var invoiceResponse = new OrderResponse();
                var newTransaction = new PaymentTransaction();
                PaymentTransaction? currTran = null;

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
                    string vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
                    string vnp_BankCode = vnpay.GetResponseData("vnp_BankCode");
                    string orderId = vnpay.GetResponseData("vnp_TxnRef");
                    string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                    string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                    string vnp_SecureHash = httpRequest.Query["vnp_SecureHash"]!;
                    int vnp_Amount = Convert.ToInt32(vnpay.GetResponseData("vnp_Amount")) / 100;

                    var payment = await _context
                        .Payments
                        .Where(p => p.PaymentOrderId.ToString().ToLower().Equals(orderId.ToString().ToLower()))
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Payment not found.");

                    if (!string.IsNullOrEmpty(vnpayTranId))
                    {
                        currTran = await _context
                                                  .PaymentTransactions
                                                  .Where(pt => pt.TranPayload.ToLower().Equals(vnpayTranId.ToLower()) && pt.PaymentId == payment.PaymentId)
                                                  .FirstOrDefaultAsync(cancellationToken);
                    }
                   
                    var order = await _context
                                             .Orders
                                             .Where(o => o.OrderId.ToString().ToLower().Equals(orderId.ToString().ToLower()))
                                             .Include(o => o.Coupon)
                                             .Include(o => o.OrderDetails)
                                             .ThenInclude(od => od.Product)
                                             .FirstOrDefaultAsync(cancellationToken)
                                             ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Order not found.");
                    var destination = await _context
                                                    .PaymentDestinations
                                                    .Where(pd => pd.DesShortName.ToLower().Equals(vnp_BankCode.ToLower()))
                                                    .FirstOrDefaultAsync(cancellationToken)
                                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Destination not found.");
                    bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                    if (checkSignature)
                    {
                        if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                        {

                            //thanh toan thanh cong
                            invoiceResponse.Message = "Order payment successfully.";
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
                            invoiceResponse.TotalDiscount = order.TotalDiscount;
                            if (currTran is null)
                            {
                                var orderDetailsGroupedByProductId = order.OrderDetails
                                                                                      .GroupBy(od => od.ProductId)
                                                                                      .Select(g => new
                                                                                       {
                                                                                           ProductId = g.Key,
                                                                                           TotalQuantity = g.Sum(od => od.QuantityProduct)
                                                                                       })
                                                                                      .ToList();

                                // Batch update the StockQuantity for each ProductStock entry
                                foreach (var detail in orderDetailsGroupedByProductId)
                                {
                                    await _context.ProductStocks
                                        .Where(ps => ps.ProductId == detail.ProductId)
                                        .UpdateAsync(ps => new
                                        {
                                            StockQuantity = ps.StockQuantity - detail.TotalQuantity
                                        }, cancellationToken);
                                }
                                var paymentDate = vnpay.GetResponseData("vnp_PayDate");

                                string formatString = "yyyyMMddHHmmss";
                                DateTime dt = DateTime.ParseExact(paymentDate, formatString, null);

                                var beneficiaryName = "MT STORE";
                                var orderInfo = vnpay.GetResponseData("vnp_OrderInfo");

                                var message = new Message(order.Email, order.FullName, "Biên lai thanh toán", $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <title>Invoice</title>\r\n</head>\r\n\r\n<body>\r\n  <div\r\n    style=\"min-width:100%;width:100%!important;min-width:300px;max-width:100%;margin:0 auto;font-family:'SF Pro Text',Arial,sans-serif;min-height:100%;padding:0px;background-color:#e8e8e8\"\r\n    bgcolor=\"#e8e8e8\">\r\n    <div class=\"adM\">\r\n    </div>\r\n    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n      <tbody>\r\n        <tr>\r\n          <td align=\"center\" border=\"0\">\r\n\r\n            <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n              <tbody>\r\n                <tr>\r\n                  <td align=\"center\" border=\"0\">\r\n                    <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                      <tbody>\r\n                        <tr>\r\n                          <td align=\"center\"\r\n                            style=\"margin-left:0px;margin-right:0px;padding:20px 0px 20px 0px\"\r\n                            border=\"0\">\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n\r\n            <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width:800px\"\r\n              border=\"0\">\r\n              <tbody>\r\n                <tr>\r\n                  <td align=\"center\" style=\"background-color:#ffffff\" bgcolor=\"#ffffff\"\r\n                    border=\"0\">\r\n\r\n                    <table style=\"max-width:740px;width:100%\" cellpadding=\"0\"\r\n                      cellspacing=\"0\" border=\"0\">\r\n                      <tbody>\r\n                        <tr>\r\n                          <td align=\"center\"\r\n                            style=\"margin-left:0px;margin-right:0px;padding:0px 15px 0px 15px\"\r\n                            border=\"0\">\r\n                            <p></p>\r\n                            <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\"\r\n                              border=\"0\">\r\n                              <tbody>\r\n                                <tr>\r\n                                  <td\r\n                                    style=\"border-bottom:3px solid #0056a6;padding:20px 0px 20px 0px\">\r\n                                    <table style=\"width:100%\" cellpadding=\"0\"\r\n                                      cellspacing=\"0\" border=\"0\">\r\n                                      <tbody>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-size:0px;padding:0px 1px 0px 1px\"\r\n                                            border=\"0\">\r\n                                            <table style=\"width:100%\" cellpadding=\"0\"\r\n                                              cellspacing=\"0\" border=\"0\">\r\n                                              <tbody>\r\n                                                <tr>\r\n                                                  <td align=\"left\" valign=\"middle\"\r\n                                                    style=\"width:68%;font-size:0px;min-height:1px\"\r\n                                                    border=\"0\">\r\n                                                    <table\r\n                                                      style=\"width:100%;max-width:100%;border-collapse:collapse;word-break:break-word\"\r\n                                                      cellpadding=\"0\" cellspacing=\"0\"\r\n                                                      border=\"0\">\r\n                                                      <tbody>\r\n                                                        <tr>\r\n                                                          <td width=\"21\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"tel:1900545413\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;text-align:left;text-decoration:none;color:#4a4a4a\"\r\n                                                              target=\"_blank\">\r\n                                                              <img\r\n                                                                src=\"https://t3.ftcdn.net/jpg/05/41/87/60/360_F_541876040_o8471YjjddENbmwJvS1OdhWkWSy2dOyW.jpg\"\r\n                                                                width=\"16\" alt=\"Vnpay\"\r\n                                                                style=\"width:18px;display:inline-block;height:auto\"\r\n                                                                border=\"0\" height=\"16\"\r\n                                                                class=\"CToWUd\"\r\n                                                                data-bit=\"iit\">\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td width=\"130\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"tel:1900545413\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;text-decoration:none;color:#00381a\"\r\n                                                              target=\"_blank\">\r\n                                                              1900 55 55 77\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td\r\n                                                            style=\"border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\"></td>\r\n                                                        </tr>\r\n                                                        <tr>\r\n                                                          <td width=\"21\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"https://vnpay.vn\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;text-align:left;text-decoration:none;color:#4a4a4a\"\r\n                                                              target=\"_blank\"\r\n                                                              data-saferedirecturl=\"https://www.google.com/url?q=https://vnpay.vn&amp;source=gmail&amp;ust=1711691927769000&amp;usg=AOvVaw0Vk5saBmtrD_fNDD4KJhqT\">\r\n                                                              <img\r\n                                                                src=\"https://previews.123rf.com/images/photoart23d/photoart23d1901/photoart23d190101014/116302376-globe-symbol-icon-red-simple-isolated-vector-illustration.jpg\"\r\n                                                                width=\"17\" alt=\"Vnpay\"\r\n                                                                style=\"width:17px;max-width:100%;display:inline-block;height:auto\"\r\n                                                                border=\"0\" height=\"17\"\r\n                                                                class=\"CToWUd\"\r\n                                                                data-bit=\"iit\">\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td width=\"230\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"https://vnpay.vn\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;text-decoration:none;color:#00381a\"\r\n                                                              target=\"_blank\"\r\n                                                              data-saferedirecturl=\"https://www.google.com/url?q=https://vnpay.vn&amp;source=gmail&amp;ust=1711691927769000&amp;usg=AOvVaw0Vk5saBmtrD_fNDD4KJhqT\">\r\n                                                              https://portal.vnpay.<wbr>vn\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td\r\n                                                            style=\"border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\"></td>\r\n                                                        </tr>\r\n                                                      </tbody>\r\n                                                    </table>\r\n                                                  </td>\r\n                                                  <td align=\"left\" valign=\"middle\"\r\n                                                    style=\"width:32%;font-size:0px;min-height:1px\"\r\n                                                    border=\"0\">\r\n                                                    <p\r\n                                                      style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:12px;line-height:16px;text-align:right;margin:0px;color:#4a4a4a\">\r\n                                                      <img\r\n                                                        src=\"https://seeklogo.com/images/V/vnpay-logo-CCF12E3F02-seeklogo.com.png\"\r\n                                                        width=\"135\" alt=\"Vnpay\"\r\n                                                        style=\"width:135px;max-width:100%;display:inline-block;height:auto\"\r\n                                                        border=\"0\" height=\"54.5\"\r\n                                                        class=\"CToWUd\" data-bit=\"iit\">\r\n                                                    </p>\r\n                                                  </td>\r\n                                                </tr>\r\n                                              </tbody>\r\n                                            </table>\r\n                                          </td>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td align=\"left\" style=\"padding:30px 0px 20px 0px\"\r\n                                    border=\"0\">\r\n                                    <p\r\n                                      style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:24px;line-height:29px;text-align:center;margin:0px;color:#4a4a4a\">\r\n                                      <b>Biên lai thanh toán</b>\r\n                                      <br>\r\n                                      <b style=\"font-size:16px;line-height:24px\">(Payment\r\n                                        Receipt)</b>\r\n                                    </p>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td align=\"left\"\r\n                                    style=\"font-size:14px;line-height:20px;padding:0px 0px 30px 0px\"\r\n                                    border=\"0\">\r\n                                    <table\r\n                                      style=\"width:100%;max-width:100%;border-collapse:collapse;word-break:break-word;min-width:640px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5\"\r\n                                      cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                      <tbody>\r\n                                        <tr>\r\n                                          <td width=\"200\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Ngày, giờ giao dịch</b><br>\r\n                                            <i>Trans. Date, Time</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {string.Format("{0:G}", dt)}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Số lệnh giao dịch</b><br>\r\n                                            <i>Order Number</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {orderId}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Tên người hưởng</b><br>\r\n                                            <i>Beneficiary Name </i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {beneficiaryName}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Mã hóa đơn/Mã khách hàng</b><br>\r\n                                            <i>Bill code/Customer code</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {vnpayTranId}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Số tiền</b><br>\r\n                                            <i>Amount</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {vnp_Amount.ToString("N0", new CultureInfo("vi-VN"))} VND</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Nội dung thanh toán</b><br>\r\n                                            <i>Details of Payment </i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {orderInfo}\r\n                                          </td>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td style=\"padding:0px 0px 30px 0px\" border=\"0\"\r\n                                    align=\"left\">\r\n                                    <p\r\n                                      style=\"font-family:'SF Pro Text','Arial',sans-serif;font-size:16px;line-height:24px;text-align:center;margin:0px;color:#4a4a4a\">\r\n                                      <b>Cám ơn Quý khách đã sử dụng dịch vụ của\r\n                                        Vnpay!</b><br><i>Thank you for banking with\r\n                                        Vnpay!</i>\r\n                                    </p>\r\n                                  </td>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n          </td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n\r\n\r\n    <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width:800px\"\r\n      border=\"0\">\r\n      <tbody>\r\n        <tr>\r\n          <td align=\"center\" style=\"padding:0px 0px 30px 0px\" border=\"0\">\r\n          </td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n\r\n    </td>\r\n    </tr>\r\n    </tbody>\r\n    </table>\r\n    <div class=\"yj6qo\"></div>\r\n    <div class=\"adL\">\r\n    </div>\r\n  </div>\r\n</body>\r\n\r\n</html>");

                                await _mailservice.SendEmailAsync(message, cancellationToken);
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
                                payment.PaymentLastMessage = "Order payment successfully";
                                payment.PaymentDestinationId = destination.DestinationId;

                                //response
                                invoiceResponse.TranId = newTransaction?.TranscationId;

                                //update trang thai order
                                order.Status = OrderStatus.Succeed;
                                await _context.SaveChangesAsync(cancellationToken);
                                await transaction.CommitAsync(cancellationToken);
                                await _hubcontext.Clients.All.SendAsync("ReceivedOrder", order, cancellationToken);
                            }
                        }
                        else
                        {
                            //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                            var errorMessage = "Order payment failed.";

                            if (currTran is null)
                            {
                                payment.PaymentLastMessage = errorMessage;
                                payment.PaymentStatus = PaymentStatus.Failed;
                                order.Status = OrderStatus.Failed;
                                newTransaction.TranscationId = Guid.NewGuid();
                                newTransaction.TranPayload = vnpayTranId;
                                newTransaction.TranStatus = vnp_TransactionStatus;
                                newTransaction.TranMessage = errorMessage;
                                newTransaction.TranAmount = vnp_Amount;
                                newTransaction.CreatedAt = DateTime.Now;
                                newTransaction.PaymentId = payment.PaymentId;
                                invoiceResponse.TranId = newTransaction?.TranscationId;
                                await _context.PaymentTransactions.AddAsync(newTransaction, cancellationToken);
                                await _context.SaveChangesAsync(cancellationToken);
                                await transaction.CommitAsync(cancellationToken);
                            }
                            throw new HttpStatusException(HttpStatusCode.BadRequest, errorMessage);
                        }
                    }
                    else
                    {
                        var errorMessage = "An error occurred during processing.";
                        if (currTran is null)
                        {
                            payment.PaymentLastMessage = errorMessage;
                            payment.PaymentStatus = PaymentStatus.Failed;
                            order.Status = OrderStatus.Failed;
                            newTransaction.TranscationId = Guid.NewGuid();
                            newTransaction.TranPayload = vnpayTranId;
                            newTransaction.TranStatus = vnp_TransactionStatus;
                            newTransaction.TranMessage = errorMessage;
                            newTransaction.TranAmount = vnp_Amount;
                            newTransaction.CreatedAt = DateTime.Now;
                            newTransaction.PaymentId = payment.PaymentId;
                            invoiceResponse.TranId = newTransaction?.TranscationId;
                            await _context.PaymentTransactions.AddAsync(newTransaction, cancellationToken);
                            await _context.SaveChangesAsync(cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                        }
                        throw new HttpStatusException(HttpStatusCode.BadRequest, errorMessage);
                    }
                }
                return invoiceResponse;
            }
            catch(HttpStatusException hse)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new HttpStatusException(hse.Status, hse.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PaymentResponse> PostPaymentVnPayOrderAsync(PaymentDto paymentDto,HttpRequest httpRequest ,CancellationToken cancellationToken)
        {
            using (var newTransaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    //Thống kê theo destination
                    {
                        //var merchantOfCurrentPayment = await _context
                        //                    .Merchants
                        //                    .Where(m => m.MerchantId == paymentDto.MerchantId)
                        //                    .FirstOrDefaultAsync(cancellationToken)
                        //                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Merchant not found.");
                        //var destinationOfCurrentPayment = await _context
                        //                                                .PaymentDestinations
                        //                                                .Where(pd => pd.DestinationId == paymentDto.DestinationId)
                        //                                                .FirstOrDefaultAsync(cancellationToken)
                        //                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Destinate not found.");
                    }
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

                    var listOrderDetail = new List<OrderDetail>();
                    foreach(var od in paymentDto.OrderDetails) {
                        var newOrderDetail = new OrderDetail()
                        {
                            OrderId = newOrder.OrderId,
                            ProductId = od.ProductId,
                            PriceProduct = od.PriceProduct,
                            QuantityProduct = od.QuantityProduct,
                            DiscountProduct = od.DiscountProduct,
                            Color = od.Color,
                        };
                        listOrderDetail.Add(newOrderDetail);
                    }

                    var newPayment = new Payment()
                    {
                        PaymentId = Guid.NewGuid(),
                        PaymentContent = "Thanh toan don hang : " + newOrder.OrderId.ToString(),
                        PaymentCurrency = "VND",
                        RequiredAmount = paymentDto.RequiredAmount,
                        PaymentLanguage = "vn",
                        PaymentLastMessage = "Payment order processing.",
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
                    //vnpay.AddRequestData("vnp_BankCode", "NCB");
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
                    if (!string.IsNullOrEmpty(fullName))
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
                    await newTransaction.RollbackAsync(cancellationToken);
                    throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        public async Task<IpnResponse> GetPaymentVnPayIpnAsync(HttpRequest httpRequest, CancellationToken cancellationToken)
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

        public async Task<OrderResponse> PostPaymentPayOSReturnAsync(HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            PaymentTransaction? currTran = null;
            Payment? payment = null;
            Order? order = null;
            var newTransaction = new PaymentTransaction();
            var invoiceResponse = new OrderResponse();
            int tranAmount = 0;
            Transaction? payOSTransaction = null;
            try
            {
                if (httpRequest.Query.Count > 0)
                {
                    var payOSData = httpRequest.Query;

                    string? code = payOSData["code"];
                    string? id = payOSData["id"];
                    string? cancel = payOSData["cancel"];
                    string? status = payOSData["status"];
                    string? orderCode = payOSData["orderCode"];

                    if(string.IsNullOrEmpty(code) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(cancel) || string.IsNullOrEmpty(status) || string.IsNullOrEmpty(orderCode))
                    {
                        throw new HttpStatusException(HttpStatusCode.BadRequest, "The parameter in return url not provide.");
                    }

                    var paymentLinkData = await _payoslibrary._payOS.getPaymentLinkInformation(long.Parse(orderCode));

                    order = await _context
                                             .Orders
                                             .Where(o => o.OrderCode.ToString().Equals(orderCode))
                                             .Include(o => o.Coupon)
                                             .Include(o => o.OrderDetails)
                                             .ThenInclude(od => od.Product)
                                             .FirstOrDefaultAsync(cancellationToken)
                                             ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Order not found.");
                    payment = await _context
                                            .Payments
                                            .Where(p => p.PaymentOrderId.ToString().ToLower().Equals(order.OrderId.ToString().ToLower()))
                                            .FirstOrDefaultAsync(cancellationToken)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Payment not found.");

                    tranAmount = paymentLinkData.amount;

                    if (paymentLinkData.transactions.Count > 0)
                    {
                        payOSTransaction = paymentLinkData.transactions[0];
                        var payOSReference = payOSTransaction.reference.ToString().ToLower();
                        currTran = await _context.PaymentTransactions
                                                 .Where(pt => pt.TranPayload.ToLower() == payOSReference)
                                                 .FirstOrDefaultAsync(cancellationToken);
                    }
                    if(paymentLinkData.status == "CANCELLED")
                    {
                        throw new PayOSError("01", "Order payment failed.");
                    }
                   
                    var destination = await _context
                                                    .PaymentDestinations
                                                    .Where(pd => pd.DesShortName.ToLower().Equals("OCB"))
                                                    .FirstOrDefaultAsync(cancellationToken)
                                                    ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Destination not found.");
                    //thanh toan thanh cong
                    invoiceResponse.Message = "Order payment successfully.";
                    invoiceResponse.OrderCode = long.Parse(orderCode);
                    invoiceResponse.OrderId = order.OrderId;
                    invoiceResponse.Amount = paymentLinkData.amount;
                    if(payOSTransaction != null)
                    {
                        invoiceResponse.TranNo = payOSTransaction.reference;
                    }
                    else
                    {
                        invoiceResponse.TranNo = "0";
                    }
                    invoiceResponse.CustomerId = order.UserId;
                    invoiceResponse.CustomerEmail = order.Email;
                    invoiceResponse.CustomerName = order.FullName;
                    invoiceResponse.CustomerPhone = order.PhoneNumber;
                    invoiceResponse.InvoiceAddress = order.DeliveryAddress;
                    invoiceResponse.InvoiceDate = order.OrderDate;
                    invoiceResponse.OrderDetails = order.OrderDetails;
                    invoiceResponse.TotalDiscount = order.TotalDiscount;
                    if (currTran is null)
                    {
                        var paymentDate = paymentLinkData.createdAt;

                        string format = "yyyy-MM-ddTHH:mm:ssK"; // Format to handle the timezone
                        DateTime dt = DateTime.ParseExact(paymentDate, format, null);

                        var beneficiaryName = "MT STORE";

                        var message = new Message(order.Email, order.FullName, "Biên lai thanh toán", $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <title>Invoice</title>\r\n</head>\r\n\r\n<body>\r\n  <div\r\n    style=\"min-width:100%;width:100%!important;min-width:300px;max-width:100%;margin:0 auto;font-family:'SF Pro Text',Arial,sans-serif;min-height:100%;padding:0px;background-color:#e8e8e8\"\r\n    bgcolor=\"#e8e8e8\">\r\n    <div class=\"adM\">\r\n    </div>\r\n    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n      <tbody>\r\n        <tr>\r\n          <td align=\"center\" border=\"0\">\r\n\r\n            <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n              <tbody>\r\n                <tr>\r\n                  <td align=\"center\" border=\"0\">\r\n                    <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                      <tbody>\r\n                        <tr>\r\n                          <td align=\"center\"\r\n                            style=\"margin-left:0px;margin-right:0px;padding:20px 0px 20px 0px\"\r\n                            border=\"0\">\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n\r\n            <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width:800px\"\r\n              border=\"0\">\r\n              <tbody>\r\n                <tr>\r\n                  <td align=\"center\" style=\"background-color:#ffffff\" bgcolor=\"#ffffff\"\r\n                    border=\"0\">\r\n\r\n                    <table style=\"max-width:740px;width:100%\" cellpadding=\"0\"\r\n                      cellspacing=\"0\" border=\"0\">\r\n                      <tbody>\r\n                        <tr>\r\n                          <td align=\"center\"\r\n                            style=\"margin-left:0px;margin-right:0px;padding:0px 15px 0px 15px\"\r\n                            border=\"0\">\r\n                            <p></p>\r\n                            <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\"\r\n                              border=\"0\">\r\n                              <tbody>\r\n                                <tr>\r\n                                  <td\r\n                                    style=\"border-bottom:3px solid #0056a6;padding:20px 0px 20px 0px\">\r\n                                    <table style=\"width:100%\" cellpadding=\"0\"\r\n                                      cellspacing=\"0\" border=\"0\">\r\n                                      <tbody>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-size:0px;padding:0px 1px 0px 1px\"\r\n                                            border=\"0\">\r\n                                            <table style=\"width:100%\" cellpadding=\"0\"\r\n                                              cellspacing=\"0\" border=\"0\">\r\n                                              <tbody>\r\n                                                <tr>\r\n                                                  <td align=\"left\" valign=\"middle\"\r\n                                                    style=\"width:68%;font-size:0px;min-height:1px\"\r\n                                                    border=\"0\">\r\n                                                    <table\r\n                                                      style=\"width:100%;max-width:100%;border-collapse:collapse;word-break:break-word\"\r\n                                                      cellpadding=\"0\" cellspacing=\"0\"\r\n                                                      border=\"0\">\r\n                                                      <tbody>\r\n                                                        <tr>\r\n                                                          <td width=\"21\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"tel:1900545413\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;text-align:left;text-decoration:none;color:#4a4a4a\"\r\n                                                              target=\"_blank\">\r\n                                                              <img\r\n                                                                src=\"https://t3.ftcdn.net/jpg/05/41/87/60/360_F_541876040_o8471YjjddENbmwJvS1OdhWkWSy2dOyW.jpg\"\r\n                                                                width=\"16\" alt=\"Vnpay\"\r\n                                                                style=\"width:18px;display:inline-block;height:auto\"\r\n                                                                border=\"0\" height=\"16\"\r\n                                                                class=\"CToWUd\"\r\n                                                                data-bit=\"iit\">\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td width=\"130\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"tel:1900545413\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;text-decoration:none;color:#00381a\"\r\n                                                              target=\"_blank\">\r\n                                                              1900 55 55 77\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td\r\n                                                            style=\"border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\"></td>\r\n                                                        </tr>\r\n                                                        <tr>\r\n                                                          <td width=\"21\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"https://vnpay.vn\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;text-align:left;text-decoration:none;color:#4a4a4a\"\r\n                                                              target=\"_blank\"\r\n                                                              data-saferedirecturl=\"https://www.google.com/url?q=https://vnpay.vn&amp;source=gmail&amp;ust=1711691927769000&amp;usg=AOvVaw0Vk5saBmtrD_fNDD4KJhqT\">\r\n                                                              <img\r\n                                                                src=\"https://previews.123rf.com/images/photoart23d/photoart23d1901/photoart23d190101014/116302376-globe-symbol-icon-red-simple-isolated-vector-illustration.jpg\"\r\n                                                                width=\"17\" alt=\"Vnpay\"\r\n                                                                style=\"width:17px;max-width:100%;display:inline-block;height:auto\"\r\n                                                                border=\"0\" height=\"17\"\r\n                                                                class=\"CToWUd\"\r\n                                                                data-bit=\"iit\">\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td width=\"230\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"https://vnpay.vn\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;text-decoration:none;color:#00381a\"\r\n                                                              target=\"_blank\"\r\n                                                              data-saferedirecturl=\"https://www.google.com/url?q=https://vnpay.vn&amp;source=gmail&amp;ust=1711691927769000&amp;usg=AOvVaw0Vk5saBmtrD_fNDD4KJhqT\">\r\n                                                              https://portal.vnpay.<wbr>vn\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td\r\n                                                            style=\"border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\"></td>\r\n                                                        </tr>\r\n                                                      </tbody>\r\n                                                    </table>\r\n                                                  </td>\r\n                                                  <td align=\"left\" valign=\"middle\"\r\n                                                    style=\"width:32%;font-size:0px;min-height:1px\"\r\n                                                    border=\"0\">\r\n                                                    <p\r\n                                                      style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:12px;line-height:16px;text-align:right;margin:0px;color:#4a4a4a\">\r\n                                                      <img\r\n                                                        src=\"https://seeklogo.com/images/V/vnpay-logo-CCF12E3F02-seeklogo.com.png\"\r\n                                                        width=\"135\" alt=\"Vnpay\"\r\n                                                        style=\"width:135px;max-width:100%;display:inline-block;height:auto\"\r\n                                                        border=\"0\" height=\"54.5\"\r\n                                                        class=\"CToWUd\" data-bit=\"iit\">\r\n                                                    </p>\r\n                                                  </td>\r\n                                                </tr>\r\n                                              </tbody>\r\n                                            </table>\r\n                                          </td>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td align=\"left\" style=\"padding:30px 0px 20px 0px\"\r\n                                    border=\"0\">\r\n                                    <p\r\n                                      style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:24px;line-height:29px;text-align:center;margin:0px;color:#4a4a4a\">\r\n                                      <b>Biên lai thanh toán</b>\r\n                                      <br>\r\n                                      <b style=\"font-size:16px;line-height:24px\">(Payment\r\n                                        Receipt)</b>\r\n                                    </p>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td align=\"left\"\r\n                                    style=\"font-size:14px;line-height:20px;padding:0px 0px 30px 0px\"\r\n                                    border=\"0\">\r\n                                    <table\r\n                                      style=\"width:100%;max-width:100%;border-collapse:collapse;word-break:break-word;min-width:640px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5\"\r\n                                      cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                      <tbody>\r\n                                        <tr>\r\n                                          <td width=\"200\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Ngày, giờ giao dịch</b><br>\r\n                                            <i>Trans. Date, Time</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {string.Format("{0:G}", dt)}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Số lệnh giao dịch</b><br>\r\n                                            <i>Order Number</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {order.OrderCode}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Tên người hưởng</b><br>\r\n                                            <i>Beneficiary Name </i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {beneficiaryName}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Mã hóa đơn/Mã khách hàng</b><br>\r\n                                            <i>Bill code/Customer code</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {invoiceResponse.TranNo}</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Số tiền</b><br>\r\n                                            <i>Amount</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            {paymentLinkData.amount.ToString("N0", new CultureInfo("vi-VN"))} VND</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Nội dung thanh toán</b><br>\r\n                                            <i>Details of Payment </i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            Thanh toan don hang: {orderCode}\r\n                                          </td>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td style=\"padding:0px 0px 30px 0px\" border=\"0\"\r\n                                    align=\"left\">\r\n                                    <p\r\n                                      style=\"font-family:'SF Pro Text','Arial',sans-serif;font-size:16px;line-height:24px;text-align:center;margin:0px;color:#4a4a4a\">\r\n                                      <b>Cám ơn Quý khách đã sử dụng dịch vụ của\r\n                                        Vnpay!</b><br><i>Thank you for banking with\r\n                                        Vnpay!</i>\r\n                                    </p>\r\n                                  </td>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n          </td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n\r\n\r\n    <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width:800px\"\r\n      border=\"0\">\r\n      <tbody>\r\n        <tr>\r\n          <td align=\"center\" style=\"padding:0px 0px 30px 0px\" border=\"0\">\r\n          </td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n\r\n    </td>\r\n    </tr>\r\n    </tbody>\r\n    </table>\r\n    <div class=\"yj6qo\"></div>\r\n    <div class=\"adL\">\r\n    </div>\r\n  </div>\r\n</body>\r\n\r\n</html>");

                        await _mailservice.SendEmailAsync(message, cancellationToken);
                        //tao transac
                        newTransaction.TranscationId = Guid.NewGuid();
                        newTransaction.TranPayload = payOSTransaction.reference;
                        newTransaction.TranStatus = "00";
                        newTransaction.TranMessage = invoiceResponse.Message;
                        newTransaction.TranAmount = payOSTransaction.amount;
                       
                        newTransaction.CreatedAt = DateTime.ParseExact(payOSTransaction.transactionDateTime, format, null, DateTimeStyles.RoundtripKind);
                        newTransaction.PaymentId = payment.PaymentId;

                        //cap nhat payment message
                        payment.PaidAmout = paymentLinkData.amountPaid;
                        payment.PaymentStatus = PaymentStatus.Succeed;
                        payment.PaymentLastMessage = "Order payment successful";
                        payment.PaymentDestinationId = destination.DestinationId;

                        //response
                        invoiceResponse.TranId = newTransaction?.TranscationId;

                        //update trang thai order
                        order.Status = OrderStatus.Succeed;
                        await _hubcontext.Clients.All.SendAsync("ReceivedOrder", order, cancellationToken);
                        await _context.PaymentTransactions.AddAsync(newTransaction, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                }
                return invoiceResponse;
            }
            catch (PayOSError pse)
            {
                var errorMessage = pse.Message;

                if (payment != null && order != null && currTran is null)
                {
                    payment.PaymentLastMessage = errorMessage;
                    payment.PaymentStatus = PaymentStatus.Failed;
                    order.Status = OrderStatus.Failed;
                    newTransaction.TranscationId = Guid.NewGuid();
                    newTransaction.TranPayload = "0";
                    newTransaction.TranStatus = pse.Code;
                    newTransaction.TranMessage = pse.Message;
                    newTransaction.TranAmount = tranAmount;
                    newTransaction.CreatedAt = DateTime.Now;
                    newTransaction.PaymentId = payment.PaymentId;
                    invoiceResponse.TranId = newTransaction?.TranscationId;
                    await _context.PaymentTransactions.AddAsync(newTransaction, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                throw new HttpStatusException(HttpStatusCode.BadRequest, errorMessage);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<PaymentResponse> PostPaymentPayOSOrderAsync(PaymentDto paymentDto, HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                long orderCode = long.Parse(DateTimeOffset.Now.ToString("ffffff"));
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
                    OrderCode = orderCode,
                };
                await _context.Orders.AddAsync(newOrder, cancellationToken);

                var listOrderDetail = new List<OrderDetail>();
                foreach (var od in paymentDto.OrderDetails)
                {
                    var newOrderDetail = new OrderDetail()
                    {
                        OrderId = newOrder.OrderId,
                        ProductId = od.ProductId,
                        PriceProduct = od.PriceProduct,
                        QuantityProduct = od.QuantityProduct,
                        DiscountProduct = od.DiscountProduct,
                        Color = od.Color,
                    };
                    listOrderDetail.Add(newOrderDetail);
                }

                var newPayment = new Payment()
                {
                    PaymentId = Guid.NewGuid(),
                    PaymentContent = "Thanh toan don hang : " + orderCode.ToString(),
                    PaymentCurrency = "VND",
                    RequiredAmount = paymentDto.RequiredAmount,
                    PaymentLanguage = "vn",
                    PaymentOrderId = newOrder.OrderId,
                    MerchantId = paymentDto.MerchantId,
                    PaymentLastMessage = "Payment order processing.",
                    PaymentDestinationId = paymentDto.DestinationId,
                    CreatedAt = DateTime.Now,
                    ExpiredAt = DateTime.Now.AddMinutes(15),
                    PaymentStatus = "pending"
                };
                List<ItemData> items = new();
                foreach (var item in paymentDto.OrderDetails)
                {
                    var productName = await _context
                        .Products
                        .Where(p => p.ProductId.Equals(item.ProductId))
                        .Select(p => p.Title)
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Product not found.");
                    var productSalePrice = item.PriceProduct * (1 - (item.DiscountProduct / 100.0f));
                    var newItem = new ItemData(productName, item.QuantityProduct, (int)productSalePrice);
                    items.Add(newItem);
                }
                PaymentData paymentData = new(orderCode, paymentDto.RequiredAmount, "Thanh toan don hang", items
                    , _payoslibrary._options.PAYOS_CANCEL_URL, _payoslibrary._options.PAYOS_RETURN_URL);

                CreatePaymentResult createPayment = await _payoslibrary._payOS.createPaymentLink(paymentData);

                var signature = await GetSignatureAsync(orderCode, cancellationToken);
                if(string.IsNullOrEmpty(signature))
                {
                    throw new HttpStatusException(HttpStatusCode.BadRequest, "Signature invalid.");
                }
                var newSign = new PaymentSignature()
                {
                    SignatureId = Guid.NewGuid(),
                    SignValue = signature,
                    SignAlgorithm = "HmacSHA256",
                    CreatedAt = DateTime.Now,
                    Payment = newPayment,
                    PaymentId = newPayment.PaymentId,
                    SignOwn = paymentDto.MerchantId.ToString(),
                };
                newPayment.PaymentUrl = createPayment.checkoutUrl;

                await _context.OrderDetails.AddRangeAsync(listOrderDetail, cancellationToken);
                await _context.Payments.AddAsync(newPayment, cancellationToken);
                await _context.PaymentSignatures.AddAsync(newSign, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new PaymentResponse()
                {
                    PaymentUrl = createPayment.checkoutUrl,
                    PaymentId = newPayment.PaymentId
                };
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public Task<OrderResponse> PostPaymentPayOsWebhookUrlAsync(HttpRequest httpRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        private async Task<string?> GetSignatureAsync(long orderCode, CancellationToken cancellationToken)
        {
            try
            {
                string url = "https://api-merchant.payos.vn/v2/payment-requests/" + orderCode;
                HttpClient httpClient = new();
                JObject responseBodyJson = JObject.Parse(await (await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Headers =
                {
                    { "x-client-id", _payoslibrary._options.PAYOS_CLIENT_ID },
                    { "x-api-key", _payoslibrary._options.PAYOS_API_KEY }
                }
                }, cancellationToken)).Content.ReadAsStringAsync(cancellationToken));
                string? signature = responseBodyJson["signature"]?.ToString();
                return signature;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
