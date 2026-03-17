using System.Net.NetworkInformation;
using PingMonitorApp.Data;
using PingMonitorApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace PingMonitorApp.Services
{
    public class PingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<int, int> _downCounters = new();
        private readonly Dictionary<int, DateTime> _downSince = new();
        private readonly Dictionary<int, bool> _alertSent = new();

        public PingBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        
                        // Llamada al PA para obtener dispositivos
                        var devices = await dbContext.Devices
                            .FromSqlRaw("EXEC pa_ObtenerDispositivosActivos")
                            .ToListAsync(stoppingToken);

                        var pingTasks = devices.Select(d => PerformPingAsync(d)).ToList();
                        var results = await Task.WhenAll(pingTasks);
                        
                        foreach ((Device device, bool isUp, long latency) in results)
                        {
                            var timestamp = DateTime.Now;
                            
                            // Llamada al PA para insertar el log
                            await dbContext.Database.ExecuteSqlRawAsync(
                                "EXEC pa_InsertarLogPing @DeviceId, @IsUp, @LatencyMs",
                                new SqlParameter("@DeviceId", device.Id),
                                new SqlParameter("@IsUp", isUp),
                                new SqlParameter("@LatencyMs", latency)
                            );
                            
                            // Handle Alert Logic (La lógica de envío de correo se mantiene aquí)
                            if (!isUp)
                            {
                                if (!_downSince.ContainsKey(device.Id)) _downSince[device.Id] = timestamp;
                                
                                if (!_downCounters.ContainsKey(device.Id)) _downCounters[device.Id] = 0;
                                _downCounters[device.Id]++;
                                
                                // 🚨 Alert after 2 consecutive failures
                                if (_downCounters[device.Id] == 2 && (!_alertSent.ContainsKey(device.Id) || !_alertSent[device.Id]))
                                {
                                    await emailService.SendAlertAsync(device.Name, device.IP, false);
                                    _alertSent[device.Id] = true;
                                }
                            }
                            else
                            {
                                // ✅ Recovery alert
                                if (_alertSent.ContainsKey(device.Id) && _alertSent[device.Id])
                                {
                                    var downTime = _downSince.ContainsKey(device.Id) ? _downSince[device.Id] : (DateTime?)null;
                                    await emailService.SendAlertAsync(device.Name, device.IP, true, downTime);
                                }
                                
                                _downCounters[device.Id] = 0;
                                _downSince.Remove(device.Id);
                                _alertSent[device.Id] = false;
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
                {
                    // Shutdown in progress
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en PingBackgroundService: {ex.Message}");
                }
                
                if (!stoppingToken.IsCancellationRequested)
                {
                    try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }
        }

        private async Task<(Device Device, bool IsUp, long Latency)> PerformPingAsync(Device device)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(device.IP, 5000); // 5s timeout
                return (device, reply.Status == IPStatus.Success, reply.RoundtripTime);
            }
            catch
            {
                return (device, false, 0);
            }
        }
    }
}
