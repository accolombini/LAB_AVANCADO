namespace Forno.Api.Device;
public interface IDevice{System.Threading.Tasks.Task StartAsync(System.Threading.CancellationToken ct=default);System.Threading.Tasks.Task StopAsync(System.Threading.CancellationToken ct=default);System.Threading.Tasks.Task<string> SendAsync(string command,System.Threading.CancellationToken ct=default);Telemetry? LastTelemetry{get;}}
