using Microsoft.Extensions.Options;
using SynkTask.API.Configurations.Models;
using SynkTask.API.Services.IService;
using System.Net;
using System.Net.Mail;

namespace SynkTask.API.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSenderConfig emailSenderConfig;

        public EmailSender(IOptionsMonitor<EmailSenderConfig> optionsMonitor)
        {
            emailSenderConfig = optionsMonitor.CurrentValue;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = new SmtpClient
            {
                Host = emailSenderConfig.Host,
                Port = emailSenderConfig.Port,
                EnableSsl = emailSenderConfig.EnableSsl,
                Credentials = new NetworkCredential(
                    emailSenderConfig.Username,
                    emailSenderConfig.Password
                )
            };

            var message = new MailMessage
            {
                From = new MailAddress(emailSenderConfig.Username),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            message.To.Add(email);

            await smtp.SendMailAsync(message);
        }
    }
}