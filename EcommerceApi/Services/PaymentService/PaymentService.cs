using Azure;
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
using System.Configuration;
using System.Net;

namespace EcommerceApi.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly VnPayConfig _config;
        private readonly IConfiguration _configSetting;
        private readonly EcommerceDbContext _context;
        public PaymentService(EcommerceDbContext context, IOptions<VnPayConfig> options, IConfiguration configSetting) {
            _config = options.Value;
            _configSetting = configSetting;
            _context = context;
        }

        public async Task<PaymentResponse> GetPaymentOrder(PaymentDto paymentDto,HttpRequest httpRequest ,CancellationToken cancellationToken)
        {
            try
            {
                var newPayment = new Payment() { 
                    PaymentId = Guid.NewGuid(),

                };
                VnPayLibrary vnpay = new();
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", _config.vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", (10000 * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
                vnpay.AddRequestData("vnp_BankCode", "NCB");
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + Guid.NewGuid());
                vnpay.AddRequestData("vnp_OrderType", "130000"); //default value: other
                vnpay.AddRequestData("vnp_ReturnUrl", _config.vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", Guid.NewGuid().ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày
                //Add Params of 2.1.0 Version
                vnpay.AddRequestData("vnp_ExpireDate", paymentDto.ExpiredAt.ToString("yyyyMMddHHmmss"));
                //Billing
                vnpay.AddRequestData("vnp_Bill_Email",paymentDto.Email.Trim());
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
                return new PaymentResponse() { 
                    PaymentUrl = paymentUrl,
                    PaymentId = newPayment.PaymentId
                };
            }
            catch (Exception ex)
            {

            }
        }
    }
}
