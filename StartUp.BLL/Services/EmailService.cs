using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StartUp.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer = "smtp.example.com"; // Replace with your SMTP server
        private readonly int _smtpPort = 587; // SMTP port
        private readonly string _smtpUsername = "your-email@example.com"; // SMTP username
        private readonly string _smtpPassword = "your-email-password"; // SMTP password
        private readonly string _fromAddress = "your-email@example.com"; // From address

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Set to true if you're sending HTML emails
            };

            mailMessage.To.Add(to);

            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
