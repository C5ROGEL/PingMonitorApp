namespace PingMonitorApp.Models.Dtos
{
    public class DashboardSummaryDto
    {
        public int Total { get; set; }
        public int Up { get; set; }
        public int Down { get; set; }
    }

    public class DeviceStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string Despacho { get; set; } = string.Empty;
        public bool IsUp { get; set; }
        public long Latency { get; set; }
        public string LastPing { get; set; } = string.Empty;
        public DateTime? DownSince { get; set; }
        
        // Calculated property for JSON response
        public string? DownSinceStr => DownSince?.ToString("dd/MM HH:mm:ss");
        public double DownMinutes => DownSince.HasValue ? Math.Round((DateTime.Now - DownSince.Value).TotalMinutes) : 0;
    }

    public class ChartDataDto
    {
        public string Time { get; set; } = string.Empty;
        public int Value { get; set; }
        public long Latency { get; set; }
    }
}
