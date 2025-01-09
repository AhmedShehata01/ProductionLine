using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StartUp.DAL.Database;

namespace StartUp.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationContext _context;
        private readonly IEmailService _emailService;  // You will implement email sending logic here
        private readonly ISmsService _smsService;      // You will implement SMS sending logic here

        public NotificationService(ApplicationContext context, IEmailService emailService, ISmsService smsService)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            await _emailService.SendEmailAsync(to, subject, body);
        }

        public async Task SendSmsAsync(string to, string message)
        {
            await _smsService.SendSmsAsync(to, message);
        }

        public async Task SendNotificationAsync(string templateName, Dictionary<string, string> placeholders, string recipient, string notificationType)
        {
            // Fetch the template from the database
            var template = await _context.NotificationTemplates
                                          .FirstOrDefaultAsync(t => t.TemplateName == templateName && t.TemplateType == notificationType);

            if (template == null)
                throw new Exception("Template not found");

            // Replace placeholders with actual values
            string messageBody = template.Body;
            foreach (var placeholder in placeholders)
            {
                messageBody = messageBody.Replace("{{" + placeholder.Key + "}}", placeholder.Value);
            }

            // Send the notification
            if (notificationType == "Email")
            {
                await SendEmailAsync(recipient, template.Subject, messageBody);
            }
            else if (notificationType == "SMS")
            {
                await SendSmsAsync(recipient, messageBody);
            }
            else
            {
                throw new Exception("Unsupported notification type");
            }
        }
    }

    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendSmsAsync(string to, string message);
        Task SendNotificationAsync(string templateName, Dictionary<string, string> placeholders, string recipient, string notificationType);
    }
}
