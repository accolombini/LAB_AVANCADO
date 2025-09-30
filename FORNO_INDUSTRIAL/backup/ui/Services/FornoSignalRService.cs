using Microsoft.AspNetCore.SignalR.Client;
using Forno.Ui.Models;
using System.Text.Json;

namespace Forno.Ui.Services;

/// <summary>
/// Serviço para comunicação em tempo real com a API do Forno Industrial
/// Gerencia conexão SignalR e eventos de telemetria
/// </summary>
public class FornoSignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly ILogger<FornoSignalRService> _logger;
    private bool _disposed = false;
    private Timer? _reconnectionTimer;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    
    public event EventHandler<Telemetry>? TelemetryReceived;
    public event EventHandler<bool>? ConnectionChanged;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    
    public FornoSignalRService(ILogger<FornoSignalRService> logger)
    {
        _logger = logger;
        InitializeConnection();
        StartReconnectionTimer();
    }
    
    private void InitializeConnection()
    {
        if (_disposed) return;
        
        // Configurar conexão SignalR com a API
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5002/hubs/forno")
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10) })
            .Build();
            
        // Configurar handlers de eventos
        _hubConnection.On<JsonElement>("TelemetryUpdate", OnTelemetryReceived);
        
        _hubConnection.Closed += OnConnectionClosed;
        _hubConnection.Reconnected += OnReconnected;
        _hubConnection.Reconnecting += OnReconnecting;
    }
    
    public async Task StartAsync()
    {
        if (_disposed || _hubConnection == null) return;
        
        await _connectionSemaphore.WaitAsync();
        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("JoinMonitoring");
                
                _logger.LogInformation("🔗 Conectado ao SignalR Hub do Forno");
                ConnectionChanged?.Invoke(this, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao conectar ao SignalR Hub");
            ConnectionChanged?.Invoke(this, false);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }
    
    private void StartReconnectionTimer()
    {
        // Timer para verificar e manter conexão a cada 5 segundos
        _reconnectionTimer = new Timer(async _ => await EnsureConnectionAsync(), null, 
            TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }
    
    private async Task EnsureConnectionAsync()
    {
        if (_disposed || _hubConnection == null) return;
        
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug("🔄 Tentativa de reconexão falhou: {Error}", ex.Message);
            }
        }
    }
    
    public async Task StopAsync()
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("LeaveMonitoring");
            await _hubConnection.StopAsync();
        }
    }
    
    public async Task SendCommandAsync(string command)
    {
        if (IsConnected && _hubConnection != null)
        {
            await _hubConnection.InvokeAsync("SendCommand", command);
        }
    }
    
    private void OnTelemetryReceived(JsonElement telemetryJson)
    {
        try
        {
            var telemetry = JsonSerializer.Deserialize<Telemetry>(telemetryJson.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (telemetry != null)
            {
                TelemetryReceived?.Invoke(this, telemetry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao deserializar telemetria");
        }
    }
    
    private Task OnConnectionClosed(Exception? exception)
    {
        _logger.LogWarning("🔌 Conexão SignalR perdida: {Error}", exception?.Message);
        ConnectionChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }
    
    private Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation("🔗 Reconectado ao SignalR Hub");
        ConnectionChanged?.Invoke(this, true);
        return Task.CompletedTask;
    }
    
    private Task OnReconnecting(Exception? exception)
    {
        _logger.LogWarning("🔄 Tentando reconectar...");
        return Task.CompletedTask;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        
        _disposed = true;
        
        // Parar timer de reconexão
        _reconnectionTimer?.Dispose();
        
        if (_hubConnection != null)
        {
            await StopAsync();
            await _hubConnection.DisposeAsync();
        }
        
        _connectionSemaphore.Dispose();
    }
}
