using System;
using System.Net;
using System.Net.Mail;
using Services.Interface;

namespace Services.Implementation
{
    public class EmailSenderService : IEmailSenderService
    {
        public void sendEmail(MailAddress toAddress, string subject, string body)
        {
            var fromAddress = new MailAddress("uadesso2019@gmail.com", "UADE SSO");
            const string fromPassword = "Uade123!!";

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
