using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartUp.DAL.Entity
{
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }  // e.g., "WelcomeEmail", "PasswordReset"
        public string TemplateType { get; set; }  // e.g., "Email", "SMS"
        public string Subject { get; set; }  // For Email templates
        public string Body { get; set; }  // The message body, can include placeholders
    }
}
