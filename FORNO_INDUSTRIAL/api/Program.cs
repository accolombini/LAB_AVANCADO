using Microsoft.AspNetCore.Mvc;
using Forno.Api.Device;
using Forno.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURAÇÃO DE PORTA ==========
builder.WebHost.UseUrls("http://localhost:5002");

// ========== CONFIGURAÇÃO DE SERVIÇOS ==========

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Device Configuration
var useSimulator = builder.Configuration.GetValue("UseSimulator", true);
var serialPort = builder.Configuration.GetValue("SerialPort", "/dev/tty.usbmodem1101"); // macOS

if (useSimulator)
{
    builder.Services.AddSingleton<IDevice, SimulatedDevice>();
    Console.WriteLine("🔥 Configurado para usar SIMULADOR do Forno Industrial");
}
else
{
    builder.Services.AddSingleton<IDevice>(_ => new ArduinoDevice(serialPort));
    Console.WriteLine($"🔌 Configurado para Arduino na porta: {serialPort}");
}

// Services
builder.Services.AddHostedService<DeviceHost>();
builder.Services.AddHostedService<TelemetryBroadcastService>();
builder.Services.AddSingleton<LastLogService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Forno Industrial API", 
        Version = "v1",
        Description = "API para controle e monitoramento do Forno Industrial - MOMENTO 2"
    });
});

// SignalR
builder.Services.AddSignalR();

// CORS para desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
    
    // Política específica para SignalR (precisa de AllowCredentials)
    options.AddPolicy("SignalRPolicy", policy =>
        policy.WithOrigins("http://localhost:5001", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ========== CONFIGURAÇÃO DA APLICAÇÃO ==========

var app = builder.Build();

// Swagger (apenas em desenvolvimento)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Forno Industrial API v1");
        c.RoutePrefix = "swagger"; // Acesso via /swagger
    });
}

// CORS deve vir antes de UseRouting e aplicar a todos os endpoints
app.UseCors("SignalRPolicy");
app.UseRouting();

// SignalR Hub - com política específica para credenciais
app.MapHub<FornoHub>("/hubs/forno");

// ========== API ENDPOINTS ==========

// Health Check
app.MapGet("/health", (IDevice device, IConfiguration config) =>
{
    var simulator = config.GetValue("UseSimulator", true);
    var lastTelemetry = device.LastTelemetry?.Timestamp;
    var status = device.LastTelemetry != null ? "Healthy" : "No Data";
    
    return Results.Ok(new 
    { 
        status,
        simulator,
        lastTelemetryUtc = lastTelemetry,
        momento = "MOMENTO 2 - API Industrial"
    });
});

// Telemetria atual
app.MapGet("/api/telemetry", (IDevice device) =>
{
    if (device.LastTelemetry is null)
        return Results.NoContent();
        
    return Results.Ok(device.LastTelemetry);
});

// Telemetria formatada para dashboard
app.MapGet("/api/dashboard", (IDevice device) =>
{
    if (device.LastTelemetry is null)
        return Results.NoContent();
        
    var telemetry = device.LastTelemetry;
    
    var dashboard = new
    {
        temperatura = new
        {
            atual = telemetry.TemperaturaAtual,
            setpoint = telemetry.SetPoint,
            alarme = telemetry.TemperaturaAlarme,
            critica = telemetry.TemperaturaCritica
        },
        atuadores = new
        {
            macarico = telemetry.MacaricoLigado,
            ventilador = telemetry.VentiladorLigado,
            alarme = telemetry.AlarmeAtivo
        },
        sistema = new
        {
            ativo = telemetry.SistemaAtivo,
            interrupcaoCritica = telemetry.InterrupcaoCritica,
            estado = telemetry.Estado
        },
        timestamp = telemetry.Timestamp
    };
    
    return Results.Ok(dashboard);
});

// Último log de comunicação
app.MapGet("/api/logs/last", (LastLogService logService) =>
{
    if (logService.TimestampUtc is null)
        return Results.NoContent();
        
    return Results.Ok(new 
    { 
        command = logService.Command,
        response = logService.Response,
        timestampUtc = logService.TimestampUtc 
    });
});

// Enviar comando para dispositivo
app.MapPost("/api/command", async (
    IDevice device, 
    LastLogService logService, 
    [FromBody] CommandRequest request) =>
{
    try
    {
        var response = await device.SendAsync(request.Command);
        logService.Set(request.Command, response);
        
        return Results.Ok(new 
        { 
            command = request.Command,
            response,
            success = true,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao enviar comando: {ex.Message}");
    }
});

// Configurar setpoint
app.MapPost("/api/setpoint", async (
    IDevice device,
    [FromBody] SetpointRequest request) =>
{
    try
    {
        var command = $"SET_TEMP={request.Temperature}";
        var response = await device.SendAsync(command);
        
        return Results.Ok(new 
        { 
            setpoint = request.Temperature,
            response,
            success = true
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao alterar setpoint: {ex.Message}");
    }
});

// Parada de emergência
app.MapPost("/api/emergency-stop", async (IDevice device) =>
{
    try
    {
        var response = await device.SendAsync("EMERGENCY_STOP");
        return Results.Ok(new { message = "Parada de emergência ativada", response });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro na parada de emergência: {ex.Message}");
    }
});

// Reset do sistema
app.MapPost("/api/reset", async (IDevice device) =>
{
    try
    {
        var response = await device.SendAsync("RESET_SYSTEM");
        return Results.Ok(new { message = "Sistema resetado", response });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao resetar sistema: {ex.Message}");
    }
});

Console.WriteLine("🚀 API do Forno Industrial - MOMENTO 2");
Console.WriteLine("📡 SignalR Hub: /hubs/forno");
Console.WriteLine("🔗 Health Check: /health");
Console.WriteLine("📊 Dashboard: /api/dashboard");

app.Run();

// ========== HOSTED SERVICE ==========
public sealed class DeviceHost : IHostedService
{
    private readonly IDevice _device;
    private readonly ILogger<DeviceHost> _logger;

    public DeviceHost(IDevice device, ILogger<DeviceHost> logger)
    {
        _device = device;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔥 Iniciando comunicação com dispositivo...");
        await _device.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Parando comunicação com dispositivo...");
        await _device.StopAsync(cancellationToken);
    }
}

// ========== REQUEST MODELS ==========
public record CommandRequest(string Command);
public record SetpointRequest(double Temperature);
