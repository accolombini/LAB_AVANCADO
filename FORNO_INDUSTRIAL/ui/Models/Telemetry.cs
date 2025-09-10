namespace Forno.Ui.Models;

/// <summary>
/// Modelo de telemetria do Forno Industrial - MOMENTO 3
/// Representa os dados em tempo real recebidos da API
/// </summary>
public record Telemetry(
    double TemperaturaAtual,
    double SetPoint,
    double TemperaturaAlarme,
    double TemperaturaCritica,
    bool MacaricoLigado,
    bool VentiladorLigado,
    bool AlarmeAtivo,
    bool InterrupcaoCritica,
    bool SistemaAtivo,
    string Estado,
    DateTime Timestamp
);

/// <summary>
/// Ponto de dados para gráficos de tendência
/// </summary>
public record TemperatureDataPoint(
    DateTime Timestamp,
    double Temperature,
    string Status
);

/// <summary>
/// Configurações de alarme para interface SCADA
/// </summary>
public static class AlarmThresholds
{
    public const double TEMP_NORMAL_MIN = 1000.0;
    public const double TEMP_NORMAL_MAX = 1599.0;
    public const double TEMP_ALARME = 1600.0;
    public const double TEMP_CRITICA = 1750.0;
    public const double TEMP_MAXIMA = 1800.0;
}
