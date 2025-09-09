namespace Forno.Api.Device;

/// <summary>
/// Telemetria do Forno Industrial - MOMENTO 1
/// Temperaturas de 1000-1800°C com alarmes automáticos
/// </summary>
public record Telemetry(
    double TemperaturaAtual,        // Temperatura atual (°C)
    double SetPoint,                // Temperatura desejada (°C) 
    double TemperaturaAlarme,       // Limite de alarme (1600°C)
    double TemperaturaCritica,      // Limite crítico (1750°C)
    bool MacaricoLigado,            // Estado do maçarico
    bool VentiladorLigado,          // Estado do ventilador
    bool AlarmeAtivo,               // Alarme de temperatura crítica
    bool InterrupcaoCritica,        // Sistema em emergência
    bool SistemaAtivo,              // Sistema operacional
    string Estado,                  // AQUECENDO, RESFRIANDO, MANTENDO, EMERGENCIA
    DateTime Timestamp              // Momento da captura
);
