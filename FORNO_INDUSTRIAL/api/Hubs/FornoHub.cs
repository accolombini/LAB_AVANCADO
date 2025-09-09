using Microsoft.AspNetCore.SignalR;
using Forno.Api.Device;

namespace Forno.Api.Hubs;

/// <summary>
/// Hub SignalR para transmissão de dados em tempo real do Forno Industrial
/// Permite comunicação bidirecional entre API e interfaces web
/// </summary>
public class FornoHub : Hub
{
    /// <summary>
    /// Cliente se conecta ao grupo de monitoramento
    /// </summary>
    public async Task JoinMonitoring()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "MonitoringGroup");
        await Clients.Caller.SendAsync("Connected", "Conectado ao monitoramento do forno");
    }

    /// <summary>
    /// Cliente sai do grupo de monitoramento  
    /// </summary>
    public async Task LeaveMonitoring()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "MonitoringGroup");
    }

    /// <summary>
    /// Envia comando para o dispositivo
    /// </summary>
    public async Task SendCommand(string command)
    {
        // Este método pode ser implementado para enviar comandos via SignalR
        await Clients.Group("MonitoringGroup").SendAsync("CommandSent", command);
    }
}

/// <summary>
/// Serviço para transmitir telemetria via SignalR
/// </summary>
public class TelemetryBroadcastService : IHostedService
{
    private readonly IHubContext<FornoHub> _hubContext;
    private readonly IDevice _device;
    private readonly ILogger<TelemetryBroadcastService> _logger;

    public TelemetryBroadcastService(
        IHubContext<FornoHub> hubContext, 
        IDevice device,
        ILogger<TelemetryBroadcastService> logger)
    {
        _hubContext = hubContext;
        _device = device;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _device.TelemetryReceived += OnTelemetryReceived;
        _logger.LogInformation("🔥 Serviço de broadcast de telemetria iniciado");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _device.TelemetryReceived -= OnTelemetryReceived;
        _logger.LogInformation("🛑 Serviço de broadcast de telemetria parado");
        return Task.CompletedTask;
    }

    private async void OnTelemetryReceived(object? sender, Telemetry telemetry)
    {
        try
        {
            // Envia telemetria para todos os clientes conectados
            await _hubContext.Clients.Group("MonitoringGroup")
                .SendAsync("TelemetryUpdate", telemetry);
                
            // Log reduzido - apenas para estados críticos
            if (telemetry.InterrupcaoCritica || telemetry.AlarmeAtivo)
            {
                _logger.LogInformation("🚨 Estado crítico: {Temp}°C - {Estado}", 
                    telemetry.TemperaturaAtual, telemetry.Estado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao transmitir telemetria via SignalR");
        }
    }
}
