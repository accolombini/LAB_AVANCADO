namespace Forno.Api.Device;
public interface IDevice{Task StartAsync(CancellationToken ct=default);Task StopAsync(CancellationToken ct=default);Task<string> SendAsync(string command,CancellationToken ct=default);Telemetry? LastTelemetry{get;}}
