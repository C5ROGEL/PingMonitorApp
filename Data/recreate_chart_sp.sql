IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pa_ObtenerDatosGrafica]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pa_ObtenerDatosGrafica]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[pa_ObtenerDatosGrafica]
    @DeviceId INT,
    @Hours INT = 24
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Determinar el intervalo de agrupación (en minutos)
    DECLARE @Interval INT = 1;
    IF @Hours > 6 SET @Interval = 5;
    IF @Hours > 72 SET @Interval = 15;

    SELECT 
        -- Si son más de 24 horas, mostrar Día/Mes Hora:Minuto. Si no, solo Hora:Minuto.
        CASE 
            WHEN @Hours <= 24 THEN SUBSTRING(CONVERT(VARCHAR(20), DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Timestamp) / @Interval * @Interval, 0), 108), 1, 5)
            ELSE CONVERT(VARCHAR(5), DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Timestamp) / @Interval * @Interval, 0), 103) + ' ' + SUBSTRING(CONVERT(VARCHAR(20), DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Timestamp) / @Interval * @Interval, 0), 108), 1, 5)
        END as [Time],
        CASE WHEN AVG(CAST(IsUp AS FLOAT)) >= 0.5 THEN 1 ELSE 0 END as [Value],
        AVG(LatencyMs) as [Latency]
    FROM PingLogs
    WHERE DeviceId = @DeviceId
      AND Timestamp >= DATEADD(HOUR, -@Hours, GETDATE())
    GROUP BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Timestamp) / @Interval * @Interval, 0)
    ORDER BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Timestamp) / @Interval * @Interval, 0) ASC;
END
GO
