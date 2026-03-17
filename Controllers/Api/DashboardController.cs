using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingMonitorApp.Data;
using PingMonitorApp.Models.Dtos;

namespace PingMonitorApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            // Usando pa_ObtenerResumenDashboard
            var summary = await _context.Database
                .SqlQuery<DashboardSummaryDto>($"EXEC pa_ObtenerResumenDashboard")
                .ToListAsync();

            var result = summary.FirstOrDefault() ?? new DashboardSummaryDto();
            return Ok(new { total = result.Total, up = result.Up, down = result.Down });
        }

        [HttpGet("status-list")]
        public async Task<IActionResult> GetStatusList()
        {
            // Usando pa_ObtenerListaEstados (El cálculo de DownSince ya viene del SQL)
            var stats = await _context.Database
                .SqlQuery<DeviceStatusDto>($"EXEC pa_ObtenerListaEstados")
                .ToListAsync();

            var result = stats.Select(s => new {
                s.Id,
                s.Name,
                s.IP,
                s.Despacho,
                s.IsUp,
                s.Latency,
                s.LastPing,
                DownSince = s.DownSinceStr,
                DownMinutes = s.DownMinutes
            }).ToList();

            return Ok(result);
        }

        [HttpGet("chart/{deviceId}")]
        public async Task<IActionResult> GetChartData(int deviceId, [FromQuery] int hours = 24)
        {
            // Usando pa_ObtenerDatosGrafica
            var logs = await _context.Database
                .SqlQuery<ChartDataDto>($"EXEC pa_ObtenerDatosGrafica {deviceId}, {hours}")
                .ToListAsync();

            return Ok(logs);
        }
    }
}
