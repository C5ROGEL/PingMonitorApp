using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PingMonitorApp.Models
{
    public class PingLog
    {
        public int Id { get; set; }
        
        [Required]
        public int DeviceId { get; set; }
        
        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsUp { get; set; }
        public long LatencyMs { get; set; }
    }
}
