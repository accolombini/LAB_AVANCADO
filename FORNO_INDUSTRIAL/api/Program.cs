using Microsoft.AspNetCore.Mvc;  
using Forno.Api.Device;

var builder = WebApplication.CreateBuilder(args);
var useSim = builder.Configuration.GetValue("UseSimulator", true);

if (useSim) builder.Services.AddSingleton<IDevice, SimulatedDevice>();
else
{
    var port = builder.Configuration.GetValue("SerialPort", "COM3");
    builder.Services.AddSingleton<IDevice>(_ => new ArduinoDevice(port));
}

builder.Services.AddHostedService<DeviceHost>();
builder.Services.AddSingleton<LastLogService>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

app.MapGet("/health", (IDevice dev, IConfiguration cfg) =>
{
    var sim = cfg.GetValue("UseSimulator", true);
    var ts = dev.LastTelemetry?.Timestamp;
    return Results.Ok(new { status = "Healthy", simulator = sim, lastTelemetryUtc = ts });
});

app.MapGet("/api/telemetry", (IDevice dev) => dev.LastTelemetry is null ? Results.NoContent() : Results.Ok(dev.LastTelemetry));

app.MapGet("/api/last", (LastLogService log) =>
{
    if (log.TimestampUtc is null) return Results.NoContent();
    return Results.Ok(new { command = log.Command, response = log.Response, timestampUtc = log.TimestampUtc });
});

app.MapPost("/api/cmd",
    async (IDevice dev, LastLogService log, [FromBody] string cmd) =>   // <-- [FromBody]
{
    var resp = await dev.SendAsync(cmd);
    log.Set(cmd, resp);
    return Results.Text(resp);
});

app.Run();

public sealed class DeviceHost : IHostedService
{
    private readonly IDevice _dev;
    public DeviceHost(IDevice dev) => _dev = dev;
    public Task StartAsync(CancellationToken ct) => _dev.StartAsync(ct);
    public Task StopAsync(CancellationToken ct) => _dev.StopAsync(ct);
}
