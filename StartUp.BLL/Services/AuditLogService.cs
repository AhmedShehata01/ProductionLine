using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StartUp.DAL.Database;
using StartUp.DAL.Entity;

namespace StartUp.BLL.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationContext _context;

        public AuditLogService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task LogAuditAsync(string actionType, object newEntity, object oldEntity)
        {
            var auditLog = new AuditLog
            {
                TableName = newEntity.GetType().Name, // Captures the table/entity name
                RecordId = (int)newEntity.GetType().GetProperty("Id").GetValue(newEntity), // Gets the Id of the entity
                ActionType = actionType,
                OldValue = oldEntity != null ? JsonConvert.SerializeObject(oldEntity) : null,
                NewValue = newEntity != null ? JsonConvert.SerializeObject(newEntity) : null,
                ChangedBy = "System", // Can be replaced with the current user if needed
                ChangedDate = DateTime.Now
            };

            // Add the audit log to the database
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

    }

    public interface IAuditLogService
    {
        Task LogAuditAsync(string actionType, object newEntity, object oldEntity);
    }
}
