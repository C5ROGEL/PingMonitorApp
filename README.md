# PingMonitorApp

Este es un sistema completo de monitoreo de red desarrollado con **ASP.NET Core MVC (.NET 8/10)** siguiendo una arquitectura premium y de alto rendimiento.

## Características
- **Monitoreo ICMP Real-time**: Un servicio en segundo plano realiza pings cada 30 segundos a todas las IPs registradas.
- **Dashboard Premium**: Interfaz moderna con Alpine.js y Tailwind CSS.
- **Gráficos Dinámicos**: Visualización de histórico de conectividad (UP/DOWN) mediante Chart.js.
- **Gestión de IPs**: CRUD completo con modales de SweetAlert2 sin recarga de página.
- **Alertas por Email**: Notificación automática cuando un equipo está DOWN por más de 1 minuto.
- **Base de Datos**: SQL Server con EF Core (Code-First).

## Stack Tecnológico
- **Backend**: ASP.NET Core MVC 10.0, EF Core, MailKit.
- **Frontend**: Alpine.js, Tailwind CSS (CDN), Font Awesome 6, Chart.js, SweetAlert2.
- **Base de Datos**: SQL Server (LocalDB por defecto).

## Cómo Ejecutar
1. Abre una terminal en `C:\respos_mop\PingMonitorApp`.
2. Ejecuta el comando:
   ```bash
   dotnet run
   ```
3. El sistema se abrirá en `https://localhost:7xxx`. La base de datos se inicializará automáticamente con las 23 IPs predefinidas.

## Configuración
Edita `appsettings.json` para configurar:
- **ConnectionString**: Ajusta según tu servidor SQL Server.
- **SMTP**: Configura los datos de tu servidor de correo para las alertas.

---
Proyecto desarrollado por Antigravity.
