using System.ComponentModel.DataAnnotations;

namespace PingMonitorApp.Models
{
    public class EmailRecipient
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
