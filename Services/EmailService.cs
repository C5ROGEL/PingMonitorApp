using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace PingMonitorApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAlertAsync(string deviceName, string ip, bool isUp, DateTime? downSince = null)
        {
            var smtpHost = _config["SMTP:Host"];
            var smtpPortString = _config["SMTP:Port"];
            var smtpPort = int.TryParse(smtpPortString, out var port) ? port : 587;
            var smtpUser = _config["SMTP:User"] ?? string.Empty;
            var smtpPass = _config["SMTP:Pass"] ?? string.Empty;
            var toEmail = _config["SMTP:To"] ?? string.Empty; 

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(smtpUser))
                return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Ping Monitor MOPT", smtpUser));
            message.To.Add(new MailboxAddress("Administrador de Red", toEmail));
            
            if (!isUp)
            {
                message.Subject = $"🚨 ALERTA CRÍTICA: {deviceName} DOWN";
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: sans-serif; padding: 20px; border: 1px solid #fee2e2; border-radius: 8px; background-color: #fef2f2;'>
                        <h2 style='color: #dc2626;'>Dispositivo Fuera de Línea</h2>
                        <p>Se ha detectado que el siguiente dispositivo no responde:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Dispositivo:</td><td>{deviceName}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Dirección IP:</td><td>{ip}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Hora de Caída:</td><td>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</td></tr>
                        </table>
                        <p style='margin-top: 20px; color: #7f1d1d; font-size: 0.8em;'>Este es un mensaje automático del sistema de monitoreo MOPT.</p>
                    </div>";
                message.Body = bodyBuilder.ToMessageBody();
            }
            else
            {
                var duration = downSince.HasValue ? (DateTime.Now - downSince.Value).ToString(@"hh\:mm\:ss") : "N/A";
                message.Subject = $"✅ RECUPERACIÓN: {deviceName} UP";
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: sans-serif; padding: 20px; border: 1px solid #dcfce7; border-radius: 8px; background-color: #f0fdf4;'>
                        <h2 style='color: #16a34a;'>Dispositivo Recuperado</h2>
                        <p>El dispositivo vuelve a estar en línea:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Dispositivo:</td><td>{deviceName}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Dirección IP:</td><td>{ip}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Hora de Recuperación:</td><td>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Tiempo fuera de línea:</td><td>{duration}</td></tr>
                        </table>
                    </div>";
                message.Body = bodyBuilder.ToMessageBody();
            }

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email de alerta: {ex.Message}");
            }
        }
    }
}
