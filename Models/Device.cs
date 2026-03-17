using System.ComponentModel.DataAnnotations;

namespace PingMonitorApp.Models
{
    public class Device
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string IP { get; set; } = string.Empty;
        public string Despacho { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public ICollection<PingLog> PingLogs { get; set; } = new List<PingLog>();
    }
}
