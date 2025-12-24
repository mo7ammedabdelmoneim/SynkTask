using Microsoft.Extensions.Options;
using SynkTask.API.Configurations.Models;
using System;
using System.Net;
using System.Net.Mail;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SynkTask.API.Configurations
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