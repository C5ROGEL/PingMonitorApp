using Microsoft.EntityFrameworkCore;
using PingMonitorApp.Models;

namespace PingMonitorApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<PingLog> PingLogs { get; set; }
        public DbSet<EmailRecipient> EmailRecipients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships if needed
            modelBuilder.Entity<PingLog>()
                .HasOne(p => p.Device)
                .WithMany(d => d.PingLogs)
                .HasForeignKey(p => p.DeviceId);
        }
    }
}
