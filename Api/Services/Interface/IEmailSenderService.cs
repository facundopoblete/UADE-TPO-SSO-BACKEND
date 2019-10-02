using System;
using System.Net.Mail;

namespace Services.Interface
{
    public interface IEmailSenderService
    {
        void sendEmail(MailAddress toAddress, string subject, string body);
    }
}
