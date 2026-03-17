using PingMonitorApp.Models;
using System.Linq;

namespace PingMonitorApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Devices.Any())
            {
                return;   // DB has been seeded
            }

            var devices = new Device[]
            {
                new Device { Name = "DCMOP SS", IP = "172.18.50.72", Despacho = "SS", IsActive = true },
                new Device { Name = "GACI", IP = "172.18.100.139", Despacho = "GACI", IsActive = true },
                new Device { Name = "VMT SAN MIGUEL", IP = "18.18.40.168", Despacho = "SAN MIGUEL", IsActive = true },
                new Device { Name = "DCMOP SAN MIGUEL", IP = "192.168.68.128", Despacho = "SAN MIGUEL", IsActive = true },
                new Device { Name = "PARQUE VIAL", IP = "192.168.9.210", Despacho = "PARQUE VIAL", IsActive = true },
                new Device { Name = "VMT LA LIBERTAD 1", IP = "192.168.7.13", Despacho = "LA LIBERTAD", IsActive = true },
                new Device { Name = "VMT LA LIBERTAD 2", IP = "172.19.2.100", Despacho = "LA LIBERTAD", IsActive = true },
                new Device { Name = "SOHO CASCADAS", IP = "172.19.5.200", Despacho = "CASCADAS", IsActive = true },
                new Device { Name = "VMT GESTORES", IP = "172.19.14.22", Despacho = "GESTORES", IsActive = true },
                new Device { Name = "MONTE CARMELO", IP = "192.168.5.210", Despacho = "MONTE CARMELO", IsActive = true },
                new Device { Name = "BASCULA ZACATECOLUCA", IP = "192.168.4.177", Despacho = "ZACATECOLUCA", IsActive = true },
                new Device { Name = "NVA CONCEPCIÓN", IP = "192.168.13.30", Despacho = "CHALATENANGO", IsActive = true },
                new Device { Name = "DIOP", IP = "10.11.12.174", Despacho = "DIOP", IsActive = true },
                new Device { Name = "UCP", IP = "172.18.30.88", Despacho = "UCP", IsActive = true },
                new Device { Name = "DESPACHO", IP = "172.18.40.82", Despacho = "DESPACHO", IsActive = true },
                new Device { Name = "MANTTO VIAL", IP = "172.18.160.82", Despacho = "MANTTO VIAL", IsActive = true },
                new Device { Name = "GESTORES", IP = "172.18.120.28", Despacho = "GESTORES", IsActive = true },
                new Device { Name = "VMT SANTA TECLA RECEPCIÓN", IP = "172.20.0.13", Despacho = "SANTA TECLA", IsActive = true },
                new Device { Name = "VMT DIREC TRANSITO", IP = "172.19.2.83", Despacho = "TRANSITO", IsActive = true },
                new Device { Name = "INVERSION", IP = "172.19.2.12", Despacho = "INVERSION", IsActive = true },
                new Device { Name = "VMT EDIFICIO NUEVO", IP = "172.18.30.86", Despacho = "EDIFICIO NUEVO", IsActive = true },
                new Device { Name = "VMT SANTA ANA", IP = "172.18.180.188", Despacho = "SANTA ANA", IsActive = true },
                new Device { Name = "UNKNOWN", IP = "192.168.2.190", Despacho = "UNKNOWN", IsActive = true }
            };

            context.Devices.AddRange(devices);
            context.SaveChanges();
        }
    }
}
