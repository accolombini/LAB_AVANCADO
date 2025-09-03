using System.IO.Ports;
using System.Text;
using Forno.Api.Device;

public sealed class ArduinoDevice : IDevice, IDisposable
{
    private readonly SerialPort _port;
    private readonly StringBuilder _rx = new();
    private Telemetry? _last;

    public ArduinoDevice(string portName, int baud = 115200)
    {
        _port = new SerialPort(portName, baud)
        {
            NewLine = "\n",
            ReadTimeout = 50,
            WriteTimeout = 50
        };
        _port.DataReceived += OnData;
    }

    public Telemetry? LastTelemetry => _last;

    public Task StartAsync(CancellationToken ct = default)
    {
        if (!_port.IsOpen) _port.Open();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (_port.IsOpen) _port.Close();
        return Task.CompletedTask;
    }

    public async Task<string> SendAsync(string command, CancellationToken ct = default)
    {
        if (!_port.IsOpen) _port.Open();
        await _port.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(command + "\n"), ct);
        return "OK";
    }

    private void OnData(object? s, SerialDataReceivedEventArgs e)
    {
        try
        {
            _rx.Append(_port.ReadExisting());
            int idx;
            while ((idx = _rx.ToString().IndexOf('\n')) >= 0)
            {
                var line = _rx.ToString()[..idx].Trim();
                _rx.Remove(0, idx + 1);
                if (line.Length > 0) TryParseTelemetry(line);
            }
        }
        catch { /* ignore */ }
    }

    private void TryParseTelemetry(string line)
    {
        var parts = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
        double t=0, sp=0, lmin=0, lmax=0, lcrit=0; string mode="", state="", alarm="";
        int heater=0, fan=0; bool estop=false; long dt=0, loop=0;
        foreach (var p in parts)
        {
            var kv = p.Split('=');
            if (kv.Length != 2) continue;
            var k = kv[0]; var v = kv[1];
            switch (k)
            {
                case "T": double.TryParse(v, out t); break;
                case "SP": double.TryParse(v, out sp); break;
                case "LIM_MIN": double.TryParse(v, out lmin); break;
                case "LIM_MAX": double.TryParse(v, out lmax); break;
                case "LIM_CRIT": double.TryParse(v, out lcrit); break;
                case "MODE": mode = v; break;
                case "STATE": state = v; break;
                case "HEATER": int.TryParse(v, out heater); break;
                case "FAN": int.TryParse(v, out fan); break;
                case "ALARM": alarm = v; break;
                case "ESTOP": estop = v == "1"; break;
                case "DT_ESTOP_US": long.TryParse(v, out dt); break;
                case "LOOP_US": long.TryParse(v, out loop); break;
            }
        }
        _last = new Telemetry(t, sp, lmin, lmax, lcrit, mode, state, heater, fan, alarm, estop, dt, loop, DateTime.UtcNow);
    }

    public void Dispose() => _port.Dispose();
}
