using Forno.Api.Device;
var builder=WebApplication.CreateBuilder(args);
var useSim=builder.Configuration.GetValue("UseSimulator",true);
if(useSim) builder.Services.AddSingleton<IDevice,SimulatedDevice>();
else { var port=builder.Configuration.GetValue("SerialPort","COM3"); builder.Services.AddSingleton<IDevice>(_=>new ArduinoDevice(port)); }
builder.Services.AddHostedService<DeviceHost>();
builder.Services.AddCors(o=>o.AddDefaultPolicy(p=>p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app=builder.Build();
app.UseCors();
app.MapGet("/health", (IDevice dev, IConfiguration cfg) => Results.Ok(new { status = "Healthy", simulator = cfg.GetValue("UseSimulator", true), lastTelemetryUtc = dev.LastTelemetry?.Timestamp }));

app.MapGet("/api/telemetry",(IDevice dev)=>dev.LastTelemetry is null?Results.NoContent():Results.Ok(dev.LastTelemetry));
app.MapPost("/api/cmd",async (IDevice dev,string cmd)=>Results.Text(await dev.SendAsync(cmd)));
app.Run();
public sealed class DeviceHost:IHostedService{private readonly IDevice dev;public DeviceHost(IDevice d)=>dev=d;public Task StartAsync(CancellationToken ct)=>dev.StartAsync(ct);public Task StopAsync(CancellationToken ct)=>dev.StopAsync(ct);}
