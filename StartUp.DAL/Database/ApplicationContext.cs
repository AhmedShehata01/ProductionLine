using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StartUp.DAL.Entity;
using StartUp.DAL.Extend;

namespace StartUp.DAL.Database
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }



        public ApplicationContext(DbContextOptions<ApplicationContext> ops) : base(ops) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
