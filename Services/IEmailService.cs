namespace PingMonitorApp.Services
{
    public interface IEmailService
    {
        Task SendAlertAsync(string deviceName, string ip, bool isUp, DateTime? downSince = null);
    }
}
