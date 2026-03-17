namespace PingMonitorApp.Services
{
    public interface IEmailService
    {
        Task SendAlertAsync(string deviceName, string ip, bool isUp, DateTime? downSince = null);
        Task SendSummaryReportAsync(List<FailureReportItem> failures);
    }

    public record FailureReportItem(string Name, string IP, DateTime? DownSince);
}
