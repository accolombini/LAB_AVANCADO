namespace Forno.Api.Device;

/// <summary>
/// Interface para comunicação com dispositivos do Forno Industrial
/// Suporta Arduino real via Serial e Simulador para testes
/// </summary>
public interface IDevice
{
    /// <summary>
    /// Inicia comunicação com o dispositivo
    /// </summary>
    Task StartAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Para comunicação com o dispositivo
    /// </summary>
    Task StopAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Envia comando para o dispositivo
    /// </summary>
    Task<string> SendAsync(string command, CancellationToken ct = default);
    
    /// <summary>
    /// Última telemetria recebida do dispositivo
    /// </summary>
    Telemetry? LastTelemetry { get; }
    
    /// <summary>
    /// Evento disparado quando nova telemetria é recebida
    /// </summary>
    event EventHandler<Telemetry>? TelemetryReceived;
}
