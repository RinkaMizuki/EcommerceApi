﻿using Asp.Versioning;
using EcommerceApi.Models.Message;
using EcommerceApi.Services.MailService;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }
        [HttpGet]
        [Route("send")]
        public async Task<IActionResult> SendMailHtml(CancellationToken cancellationToken)
        {
            var message = new Message("dh52107825@student.stu.edu.vn","rinka", "test send html", "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <title>Invoice</title>\r\n</head>\r\n\r\n<body>\r\n  <div\r\n    style=\"min-width:100%;width:100%!important;min-width:300px;max-width:100%;margin:0 auto;font-family:'SF Pro Text',Arial,sans-serif;min-height:100%;padding:0px;background-color:#e8e8e8\"\r\n    bgcolor=\"#e8e8e8\">\r\n    <div class=\"adM\">\r\n    </div>\r\n    <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n      <tbody>\r\n        <tr>\r\n          <td align=\"center\" border=\"0\">\r\n\r\n            <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n              <tbody>\r\n                <tr>\r\n                  <td align=\"center\" border=\"0\">\r\n                    <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                      <tbody>\r\n                        <tr>\r\n                          <td align=\"center\"\r\n                            style=\"margin-left:0px;margin-right:0px;padding:20px 0px 20px 0px\"\r\n                            border=\"0\">\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n\r\n            <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width:800px\"\r\n              border=\"0\">\r\n              <tbody>\r\n                <tr>\r\n                  <td align=\"center\" style=\"background-color:#ffffff\" bgcolor=\"#ffffff\"\r\n                    border=\"0\">\r\n\r\n                    <table style=\"max-width:740px;width:100%\" cellpadding=\"0\"\r\n                      cellspacing=\"0\" border=\"0\">\r\n                      <tbody>\r\n                        <tr>\r\n                          <td align=\"center\"\r\n                            style=\"margin-left:0px;margin-right:0px;padding:0px 15px 0px 15px\"\r\n                            border=\"0\">\r\n                            <p></p>\r\n                            <table style=\"width:100%\" cellpadding=\"0\" cellspacing=\"0\"\r\n                              border=\"0\">\r\n                              <tbody>\r\n                                <tr>\r\n                                  <td\r\n                                    style=\"border-bottom:3px solid #0056a6;padding:20px 0px 20px 0px\">\r\n                                    <table style=\"width:100%\" cellpadding=\"0\"\r\n                                      cellspacing=\"0\" border=\"0\">\r\n                                      <tbody>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-size:0px;padding:0px 1px 0px 1px\"\r\n                                            border=\"0\">\r\n                                            <table style=\"width:100%\" cellpadding=\"0\"\r\n                                              cellspacing=\"0\" border=\"0\">\r\n                                              <tbody>\r\n                                                <tr>\r\n                                                  <td align=\"left\" valign=\"middle\"\r\n                                                    style=\"width:68%;font-size:0px;min-height:1px\"\r\n                                                    border=\"0\">\r\n                                                    <table\r\n                                                      style=\"width:100%;max-width:100%;border-collapse:collapse;word-break:break-word\"\r\n                                                      cellpadding=\"0\" cellspacing=\"0\"\r\n                                                      border=\"0\">\r\n                                                      <tbody>\r\n                                                        <tr>\r\n                                                          <td width=\"21\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"tel:1900545413\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;text-align:left;text-decoration:none;color:#4a4a4a\"\r\n                                                              target=\"_blank\">\r\n                                                              <img\r\n                                                                src=\"https://t3.ftcdn.net/jpg/05/41/87/60/360_F_541876040_o8471YjjddENbmwJvS1OdhWkWSy2dOyW.jpg\"\r\n                                                                width=\"16\" alt=\"Vnpay\"\r\n                                                                style=\"width:18px;display:inline-block;height:auto\"\r\n                                                                border=\"0\" height=\"16\"\r\n                                                                class=\"CToWUd\"\r\n                                                                data-bit=\"iit\">\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td width=\"130\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"tel:1900545413\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;text-decoration:none;color:#00381a\"\r\n                                                              target=\"_blank\">\r\n                                                              1900 55 55 77\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td\r\n                                                            style=\"border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\"></td>\r\n                                                        </tr>\r\n                                                        <tr>\r\n                                                          <td width=\"21\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"https://vnpay.vn\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;text-align:left;text-decoration:none;color:#4a4a4a\"\r\n                                                              target=\"_blank\"\r\n                                                              data-saferedirecturl=\"https://www.google.com/url?q=https://vnpay.vn&amp;source=gmail&amp;ust=1711691927769000&amp;usg=AOvVaw0Vk5saBmtrD_fNDD4KJhqT\">\r\n                                                              <img\r\n                                                                src=\"https://previews.123rf.com/images/photoart23d/photoart23d1901/photoart23d190101014/116302376-globe-symbol-icon-red-simple-isolated-vector-illustration.jpg\"\r\n                                                                width=\"17\" alt=\"Vnpay\"\r\n                                                                style=\"width:17px;max-width:100%;display:inline-block;height:auto\"\r\n                                                                border=\"0\" height=\"17\"\r\n                                                                class=\"CToWUd\"\r\n                                                                data-bit=\"iit\">\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td width=\"230\"\r\n                                                            style=\"vertical-align:middle;border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\">\r\n                                                            <a href=\"https://vnpay.vn\"\r\n                                                              style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;text-decoration:none;color:#00381a\"\r\n                                                              target=\"_blank\"\r\n                                                              data-saferedirecturl=\"https://www.google.com/url?q=https://vnpay.vn&amp;source=gmail&amp;ust=1711691927769000&amp;usg=AOvVaw0Vk5saBmtrD_fNDD4KJhqT\">\r\n                                                              https://portal.vnpay.<wbr>vn\r\n                                                            </a>\r\n                                                          </td>\r\n                                                          <td\r\n                                                            style=\"border-collapse:collapse;word-break:break-word\"\r\n                                                            border=\"0\"></td>\r\n                                                        </tr>\r\n                                                      </tbody>\r\n                                                    </table>\r\n                                                  </td>\r\n                                                  <td align=\"left\" valign=\"middle\"\r\n                                                    style=\"width:32%;font-size:0px;min-height:1px\"\r\n                                                    border=\"0\">\r\n                                                    <p\r\n                                                      style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:12px;line-height:16px;text-align:right;margin:0px;color:#4a4a4a\">\r\n                                                      <img\r\n                                                        src=\"https://seeklogo.com/images/V/vnpay-logo-CCF12E3F02-seeklogo.com.png\"\r\n                                                        width=\"135\" alt=\"Vnpay\"\r\n                                                        style=\"width:135px;max-width:100%;display:inline-block;height:auto\"\r\n                                                        border=\"0\" height=\"54.5\"\r\n                                                        class=\"CToWUd\" data-bit=\"iit\">\r\n                                                    </p>\r\n                                                  </td>\r\n                                                </tr>\r\n                                              </tbody>\r\n                                            </table>\r\n                                          </td>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td align=\"left\" style=\"padding:30px 0px 20px 0px\"\r\n                                    border=\"0\">\r\n                                    <p\r\n                                      style=\"font-family:'SF Pro Text',Arial,sans-serif;font-size:24px;line-height:29px;text-align:center;margin:0px;color:#4a4a4a\">\r\n                                      <b>Biên lai thanh toán</b>\r\n                                      <br>\r\n                                      <b style=\"font-size:16px;line-height:24px\">(Payment\r\n                                        Receipt)</b>\r\n                                    </p>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td align=\"left\"\r\n                                    style=\"font-size:14px;line-height:20px;padding:0px 0px 30px 0px\"\r\n                                    border=\"0\">\r\n                                    <table\r\n                                      style=\"width:100%;max-width:100%;border-collapse:collapse;word-break:break-word;min-width:640px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5\"\r\n                                      cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                      <tbody>\r\n                                        <tr>\r\n                                          <td width=\"200\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Ngày, giờ giao dịch</b><br>\r\n                                            <i>Trans. Date, Time</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            10:52 Thứ Năm 28/03/2024</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Số lệnh giao dịch</b><br>\r\n                                            <i>Order Number</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            5613348503</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Tài khoản nguồn</b><br>\r\n                                            <i>Debit Account</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            1032250116</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Tài khoản hưởng</b><br>\r\n                                            <i>Credit Account</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            1028936052</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Tên người hưởng</b><br>\r\n                                            <i>Beneficiary Name </i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            FOODY CORP</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Mã hóa đơn/Mã khách hàng</b><br>\r\n                                            <i>Bill code/Customer code</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            189783035</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Số tiền</b><br>\r\n                                            <i>Amount</i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            62,750 VND</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Loại phí</b><br>\r\n                                            <i>Charge Code </i>\r\n                                          </td>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            Người chuyển trả<br>\r\n                                            <i>Exclude </i>\r\n                                          </td>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Số tiền phí</b><br>\r\n                                            <i>Charge Amount<br>Net income<br>VAT</i>\r\n                                          </td>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            0 VND <br>&nbsp;<br> 0 VND <br> 0 VND</td>\r\n                                        </tr>\r\n                                        <tr>\r\n                                          <td\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            <b>Nội dung thanh toán</b><br>\r\n                                            <i>Details of Payment </i>\r\n                                          </td>\r\n                                          <td colspan=\"3\"\r\n                                            style=\"font-family:'SF Pro Text',Arial,sans-serif;border-collapse:collapse;word-break:break-word;font-size:14px;border-top:1px solid #c5c5c5;border-right:1px solid #c5c5c5;border-bottom:1px solid #c5c5c5;border-left:1px solid #c5c5c5;padding:5px 10px 5px 10px\">\r\n                                            MBVCB.5613348503.QR Pay.Thanh toan cho\r\n                                            189783035 0311828036 tu tai khoan 1032250116\r\n                                          </td>\r\n                                        </tr>\r\n                                      </tbody>\r\n                                    </table>\r\n                                  </td>\r\n                                </tr>\r\n                                <tr>\r\n                                  <td style=\"padding:0px 0px 30px 0px\" border=\"0\"\r\n                                    align=\"left\">\r\n                                    <p\r\n                                      style=\"font-family:'SF Pro Text','Arial',sans-serif;font-size:16px;line-height:24px;text-align:center;margin:0px;color:#4a4a4a\">\r\n                                      <b>Cám ơn Quý khách đã sử dụng dịch vụ của\r\n                                        Vnpay!</b><br><i>Thank you for banking with\r\n                                        Vnpay!</i>\r\n                                    </p>\r\n                                  </td>\r\n                                </tr>\r\n                              </tbody>\r\n                            </table>\r\n                          </td>\r\n                        </tr>\r\n                      </tbody>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n\r\n          </td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n\r\n\r\n    <table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width:800px\"\r\n      border=\"0\">\r\n      <tbody>\r\n        <tr>\r\n          <td align=\"center\" style=\"padding:0px 0px 30px 0px\" border=\"0\">\r\n          </td>\r\n        </tr>\r\n      </tbody>\r\n    </table>\r\n\r\n    </td>\r\n    </tr>\r\n    </tbody>\r\n    </table>\r\n    <div class=\"yj6qo\"></div>\r\n    <div class=\"adL\">\r\n    </div>\r\n  </div>\r\n</body>\r\n\r\n</html>");
            await _mailService.SendEmailAsync(message, cancellationToken);
            return Ok();
        }
    }
}
