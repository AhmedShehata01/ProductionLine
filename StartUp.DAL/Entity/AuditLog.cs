using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartUp.DAL.Entity
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string ActionType { get; set; } // "Insert", "Update", "Delete"
        public string OldValue { get; set; } // Store old value in case of Update/Delete
        public string NewValue { get; set; } // Store new value in case of Insert/Update
        public string ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
    }
}
