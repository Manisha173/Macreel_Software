using Macreel_Software.Models;
 
using System.Net;
using System.Net.Mail;

namespace Macreel_Software.Services.MailSender
{

    public class MailSender
    {
        private readonly string _fromEmail = "macreelinfosoft@gmail.com";
        private readonly string _password = "wglg owzm muts ejli";
        private readonly string _host = "smtp.gmail.com";
        private readonly int _port = 587;

        public CommonMessage SendMail(string toEmail, string subject, string body, bool isHtml = true)
        {
            CommonMessage response = new CommonMessage();

            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_fromEmail);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = isHtml;

                SmtpClient smtp = new SmtpClient(_host, _port);
                smtp.Credentials = new NetworkCredential(_fromEmail, _password);
                smtp.EnableSsl = true;

                smtp.Send(mail);

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
    }
}
