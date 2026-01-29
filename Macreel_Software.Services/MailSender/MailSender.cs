using System;
using System.Net;
using System.Net.Mail;
using Macreel_Software.Models;
using Macreel_Software.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

public class MailSender
{
    private readonly string _senderId;
    private readonly string _password;
    private readonly string _host;
    private readonly int _port;
    private readonly bool _enableSsl;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MailSender(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _senderId = configuration["MailSettings:SenderId"];
        _password = configuration["MailSettings:Password"];
        _host = configuration["MailSettings:Host"];
        _port = Convert.ToInt32(configuration["MailSettings:Port"]);
        _enableSsl = Convert.ToBoolean(configuration["MailSettings:EnableSsl"]);
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<CommonMessage> SendMailAsync(MailRequest request)
    {
        CommonMessage response = new CommonMessage();

        try
        {
            string body = GetMailBody(request.BodyType, request.Value, request.UserName, request.Password, request.otp, request.clientName);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(_senderId);
                mail.To.Add(request.ToEmail);
                mail.Subject = request.Subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                if (!string.IsNullOrEmpty(request.AttachmentPath) && File.Exists(request.AttachmentPath))
                {
                    Attachment pdfAttachment = new Attachment(request.AttachmentPath);
                    pdfAttachment.ContentType = new System.Net.Mime.ContentType("application/pdf");
                    mail.Attachments.Add(pdfAttachment);
                }

                using (SmtpClient smtp = new SmtpClient(_host, _port))
                {
                    smtp.EnableSsl = _enableSsl;
                    smtp.Credentials = new NetworkCredential(_senderId, _password);
                    await smtp.SendMailAsync(mail); 
                }
            }

            response.Status = true;
            response.Message = "Mail sent successfully";
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Message = ex.Message;
        }

        return response;
    }

    private string GetMailBody(MailBodyType type, string value = "", string username = "", string password = "", string otp = "", string clientName = "")
    {
        return type switch
        {
            MailBodyType.UserCredential => UserCredential(username, password),
            MailBodyType.RegistrationLink => RegistrationLink(value),
            MailBodyType.ForgotPassword => ForgotPasswordBody(otp),
            MailBodyType.QuatationManagement => QuatationManagement(clientName),
            MailBodyType.TaxInvoice => TaxInvoice(clientName),
            MailBodyType.PerformaInvoice => PerformaInvoice(clientName),
            _ => string.Empty
        };
    }

    private string RegistrationLink(string res)
    {
        return $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 14px;
            color: #333;
            background-color: #f0f2f5;
            margin: 0;
            padding: 20px;
        }}
        .email-container {{
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            max-width: 600px;
            margin: 20px auto;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        }}
        .email-header {{
            text-align: center;
            font-size: 24px;
            font-weight: 600;
            color: #4CAF50;
            margin-bottom: 25px;
        }}
        .email-content {{
            padding: 10px;
        }}
        .email-content p {{
            margin-bottom: 15px;
            line-height: 1.6;
        }}
        .bold {{
            font-weight: bold;
        }}
        .btn {{
            display: inline-block;
            padding: 10px 20px;
            background-color: #4CAF50;
            color: #fff !important;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 15px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
            transition: background-color 0.3s ease;
        }}
        .btn:hover {{
            background-color: #45a049;
        }}
        .email-footer {{
            margin-top: 30px;
            font-size: 12px;
            text-align: center;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            Welcome to Our Team!
        </div>

        <div class=""email-content"">
            <p>Dear <span class=""bold"">Employee</span>,</p>

            <p>
                We are thrilled to have you onboard at 
                <span class=""bold"">Macreel</span>. 
                We’re looking forward to achieving great things together.
            </p>

            <p>
                To complete your registration and activate your account,
                please click the button below:
            </p>

            <p style=""text-align: center;"">
                <a class=""btn""
                   href=""http://macreelinfo.in/employee-registration?accessId={res}""
                   target=""_blank"">
                   Register Here
                </a>
            </p>

            <p>
                Once your account is verified, you can access your dashboard
                and begin your journey with Macreel.
            </p>

            <p>
                If you have any questions, feel free to reach out to our
                support team at <span class=""bold"">hr@macreel.com</span>.
            </p>

            <p>We wish you a successful and fulfilling experience with us!</p>
        </div>

        <div class=""email-footer"">
            &copy; {DateTime.Now.Year} Macreel. All rights reserved.
        </div>
    </div>
</body>
</html>";
    }


    private string UserCredential(string username, string password)
    {
        return $@"
<html>
<head>
<style>
body {{ font-family: Arial, sans-serif; background-color: #f4f6f8; padding: 20px; }}
.container {{ max-width: 600px; background: #ffffff; margin: auto; padding: 25px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
h3 {{ color: #c62828; text-align: center; }}
table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
th, td {{ border: 1px solid #ddd; padding: 10px; }}
th {{ background-color: #f0f0f0; text-align: left; }}
.footer {{ margin-top: 25px; font-size: 13px; text-align: center; color: #777; }}
</style>
</head>
<body>
<div class='container'>
<p>Welcome to,</p>
<h3>Macreel Infosoft Pvt. Ltd.</h3>
<table>
<tr><th colspan='2'>Your registration has been completed successfully</th></tr>
<tr><td><strong>User Name</strong></td><td>{username}</td></tr>
<tr><td><strong>Password</strong></td><td>{password}</td></tr>
</table>
<div class='footer'>
<p>From Macreel Infosoft Pvt. Ltd.</p>
<p>For Login : <a href='https://macreelinfo.in' target='_blank'>https://macreelinfo.in</a></p>
<img src='https://vakiluncle.co.in/assets/img/logo.png' alt='Macreel Logo' width='120'/>
</div>
</div>
</body>
</html>";
    }

    private string ForgotPasswordBody(string otp)
    {
        return $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f6f8;
            padding: 20px;
        }}
        .container {{
            max-width: 500px;
            margin: auto;
            background: #ffffff;
            padding: 25px;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }}
        h3 {{
            color: #333;
            text-align: center;
        }}
        .otp {{
            font-size: 26px;
            font-weight: bold;
            color: #d32f2f;
            text-align: center;
            letter-spacing: 4px;
            margin: 20px 0;
        }}
        p {{
            font-size: 14px;
            line-height: 1.6;
            color: #555;
            text-align: center;
        }}
        .note {{
            font-size: 12px;
            color: #888;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h3>Password Reset OTP</h3>
        <p>Use the following One-Time Password (OTP) to reset your account password.</p>

        <div class='otp'>{otp}</div>

        <p>This OTP is valid for a limited time. Please do not share it with anyone.</p>

        <p class='note'>
            If you did not request a password reset, please ignore this email.
        </p>

        <p class='note'>– Macreel Infosoft Pvt. Ltd.</p>
    </div>
</body>
</html>";
    }


    private string QuatationManagement(string clientName)
    {
        return $@"
    <p>Dear {clientName},</p>
    <p>Please find the attached <b>Quotation</b> with detailed information about the products/services.</p>
    <p>If you have any further questions, please feel free to contact us.</p>
    <br>
    <p>Best regards,</p>
    <p>Macreel Infosoft Pvt Ltd.</p>
    <img src='https://macreel.in/assets/img/logo.png' style='width:50px;' alt='Company Logo' />";
    }


    private string PerformaInvoice(string clientName)
    {
        return $@"
    <p>Dear {clientName},</p>
    <p>Please find the attached <b>Performa Invoice</b> with detailed information about the products/services.</p>
    <p>If you have any further questions, please feel free to contact us.</p>
    <br>
    <p>Best regards,</p>
    <p>Macreel Infosoft Pvt Ltd.</p>
    <img src='https://macreel.in/assets/img/logo.png' style='width:50px;' alt='Company Logo' />";
    }

    private string TaxInvoice(string clientName)
    {
        return $@"
    <p>Dear {clientName},</p>
    <p>Please find the attached <b>Tax Invoice</b> with detailed information about the products/services.</p>
    <p>If you have any further questions, please feel free to contact us.</p>
    <br>
    <p>Best regards,</p>
    <p>Macreel Infosoft Pvt Ltd.</p>
    <img src='https://macreel.in/assets/img/logo.png' style='width:50px;' alt='Company Logo' />";
    }

}
