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
        
        // Reporte de 10 minutos
        private int _cyclesUntilReport = 10; // 10 * 1 min = 10 min
        private readonly List<FailureReportItem> _failureBatch = new();

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
                            .AsNoTracking()
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
                            
                            // Lógica de Marcadores (antes Dispositivos)
                            if (!isUp)
                            {
                                if (!_downSince.ContainsKey(device.Id)) _downSince[device.Id] = timestamp;
                                
                                if (!_downCounters.ContainsKey(device.Id)) _downCounters[device.Id] = 0;
                                _downCounters[device.Id]++;
                                
                                // Si ha fallado 2 veces seguidas y no ha sido agregado al reporte actual
                                if (_downCounters[device.Id] >= 2)
                                {
                                    if (!_alertSent.GetValueOrDefault(device.Id))
                                    {
                                        await emailService.SendAlertAsync(device.Name, device.IP, false, _downSince[device.Id]);
                                        _alertSent[device.Id] = true;
                                    }

                                    if (!_failureBatch.Any(f => f.IP == device.IP))
                                    {
                                        _failureBatch.Add(new FailureReportItem(device.Name, device.IP, _downSince[device.Id]));
                                    }
                                }
                            }
                            else
                            {
                                if (_alertSent.GetValueOrDefault(device.Id))
                                {
                                    await emailService.SendAlertAsync(device.Name, device.IP, true, _downSince.GetValueOrDefault(device.Id));
                                }

                                _downCounters[device.Id] = 0;
                                _downSince.Remove(device.Id);
                                _alertSent[device.Id] = false;
                            }
                        }

                        // Lógica de Envío de Reporte (Cada 10 min)
                        _cyclesUntilReport--;
                        if (_cyclesUntilReport <= 0)
                        {
                            if (_failureBatch.Any())
                            {
                                await emailService.SendSummaryReportAsync(new List<FailureReportItem>(_failureBatch));
                                _failureBatch.Clear();
                            }
                            _cyclesUntilReport = 10; // Reset (10 * 1 min = 10 min)
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
                    try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
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
