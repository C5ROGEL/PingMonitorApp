using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PingMonitorApp.Data;

namespace PingMonitorApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public EmailService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task SendAlertAsync(string deviceName, string ip, bool isUp, DateTime? downSince = null)
        {
            var smtpHost = _config["SMTP:Host"];
            var smtpPortString = _config["SMTP:Port"];
            var smtpPort = int.TryParse(smtpPortString, out var port) ? port : 587;
            var smtpUser = _config["SMTP:User"] ?? string.Empty;
            var smtpPass = _config["SMTP:Pass"] ?? string.Empty;
            
            // Obtener destinatarios desde la base de datos
            var recipientsRaw = await _context.EmailRecipients
                .FromSqlRaw("EXEC pa_ObtenerDestinatariosActivos")
                .ToListAsync();
            
            var recipients = recipientsRaw.Select(r => r.Email).ToList();

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || !recipients.Any())
                return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Ping Monitor MOPT", smtpUser));
            
            foreach (var email in recipients)
            {
                message.To.Add(new MailboxAddress("Administrador de Red", email));
            }
            
            if (!isUp)
            {
                var dropTime = downSince ?? DateTime.Now;
                message.Subject = $"🚨 ALERTA CRÍTICA: {deviceName} DOWN";
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: sans-serif; padding: 20px; border: 1px solid #fee2e2; border-radius: 8px; background-color: #fef2f2;'>
                        <h2 style='color: #dc2626;'>Marcador Fuera de Línea</h2>
                        <p>Se ha detectado que el siguiente marcador no responde:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Marcador:</td><td>{deviceName}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Dirección IP:</td><td>{ip}</td></tr>
                            <tr><td style='padding: 8px; font-weight: bold;'>Hora de Caída:</td><td>{dropTime:dd/MM/yyyy HH:mm:ss}</td></tr>
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
                        <h2 style='color: #16a34a;'>Marcador Recuperado</h2>
                        <p>El marcador vuelve a estar en línea:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 8px; font-weight: bold;'>Marcador:</td><td>{deviceName}</td></tr>
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
        public async Task SendSummaryReportAsync(List<FailureReportItem> failures)
        {
            var smtpHost = _config["SMTP:Host"];
            var smtpPortString = _config["SMTP:Port"];
            var smtpPort = int.TryParse(smtpPortString, out var port) ? port : 587;
            var smtpUser = _config["SMTP:User"] ?? string.Empty;
            var smtpPass = _config["SMTP:Pass"] ?? string.Empty;
            
            var recipientsRaw = await _context.EmailRecipients
                .FromSqlRaw("EXEC pa_ObtenerDestinatariosActivos")
                .ToListAsync();
            
            var recipients = recipientsRaw.Select(r => r.Email).ToList();

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || !recipients.Any())
                return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Ping Monitor MOPT", smtpUser));
            foreach (var email in recipients)
            {
                message.To.Add(new MailboxAddress("Administrador de Red", email));
            }

            message.Subject = $"📊 REPORTE DE ESTADO (10 MIN): {failures.Count} ALERTAS";
            
            var tableRows = "";
            if (failures.Count == 0)
            {
                tableRows = "<tr><td colspan='3' style='padding: 12px; text-align: center; color: #64748b;'>No se detectaron fallos en este periodo.</td></tr>";
            }
            else
            {
                foreach (var f in failures)
                {
                    tableRows += $@"
                        <tr style='border-bottom: 1px solid #e2e8f0;'>
                            <td style='padding: 10px; font-weight: bold; color: #475569;'>{f.Name}</td>
                            <td style='padding: 10px; color: #64748b;'>{f.IP}</td>
                            <td style='padding: 10px; color: #ef4444; font-weight: bold;'>{(f.DownSince.HasValue ? f.DownSince.Value.ToString("HH:mm:ss") : "Desconocido")}</td>
                        </tr>";
                }
            }

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
                <div style='font-family: sans-serif; padding: 25px; border: 1px solid #e2e8f0; border-radius: 12px; background-color: #f8fafc;'>
                    <h2 style='color: #1e293b; margin-top: 0;'>Resumen de Marcadores (Últimos 10 min)</h2>
                    <p style='color: #64748b;'>Detalle de incidencias reportadas en el lapso de tiempo monitoreado:</p>
                    
                    <table style='width: 100%; border-collapse: collapse; margin-top: 15px; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>
                        <thead>
                            <tr style='background-color: #f1f5f9;'>
                                <th style='padding: 12px; text-align: left; font-size: 12px; color: #94a3b8; text-transform: uppercase;'>Marcador</th>
                                <th style='padding: 12px; text-align: left; font-size: 12px; color: #94a3b8; text-transform: uppercase;'>Dirección IP</th>
                                <th style='padding: 12px; text-align: left; font-size: 12px; color: #94a3b8; text-transform: uppercase;'>Hora de Caída</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tableRows}
                        </tbody>
                    </table>
                    
                    <p style='margin-top: 25px; color: #94a3b8; font-size: 11px;'>Reporte generado automáticamente por PingMonitor MOPT v2.0</p>
                </div>";
            
            message.Body = bodyBuilder.ToMessageBody();

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
                Console.WriteLine($"Error enviando reporte resumen: {ex.Message}");
            }
        }
    }
}
