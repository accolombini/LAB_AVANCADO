using Forno.Ui.Models;
using System.Text.Json;
using System.Text;

namespace Forno.Ui.Services;

/// <summary>
/// Serviço HTTP para comunicação REST com a API do Forno Industrial
/// Gerencia comandos e requisições HTTP
/// </summary>
public class FornoApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FornoApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public FornoApiService(HttpClient httpClient, ILogger<FornoApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri("http://localhost:5002/");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
    
    /// <summary>
    /// Verifica status da API
    /// </summary>
    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao verificar saúde da API");
            return false;
        }
    }
    
    /// <summary>
    /// Obtém telemetria atual
    /// </summary>
    public async Task<Telemetry?> GetTelemetryAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/telemetry");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Telemetry>(json, _jsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao obter telemetria");
        }
        
        return null;
    }
    
    /// <summary>
    /// Configura novo setpoint de temperatura
    /// </summary>
    public async Task<bool> SetTemperatureAsync(double temperature)
    {
        try
        {
            var payload = new { Temperature = temperature };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/setpoint", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Setpoint alterado para {Temp}°C", temperature);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao alterar setpoint");
        }
        
        return false;
    }
    
    /// <summary>
    /// Executa parada de emergência
    /// </summary>
    public async Task<bool> EmergencyStopAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("api/emergency-stop", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogWarning("🚨 Parada de emergência ativada!");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro na parada de emergência");
        }
        
        return false;
    }
    
    /// <summary>
    /// Reset do sistema
    /// </summary>
    public async Task<bool> ResetSystemAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("api/reset", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("🔄 Sistema resetado");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao resetar sistema");
        }
        
        return false;
    }
    
    /// <summary>
    /// Envia comando personalizado
    /// </summary>
    public async Task<bool> SendCommandAsync(string command)
    {
        try
        {
            var payload = new { Command = command };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/command", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("📤 Comando enviado: {Command}", command);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar comando");
        }
        
        return false;
    }
}
