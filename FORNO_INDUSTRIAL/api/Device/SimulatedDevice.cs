using Forno.Api.Device;

public sealed class SimulatedDevice : IDevice
{
    private bool _running; private Telemetry? _last;
    private double _t = 25, _sp = 180, _lmin=120, _lmax=220, _lcrit=240;
    private string _mode = "AUTO", _state = "IDLE", _alarm="NONE"; private int _heater=0,_fan=0; private bool _estop=false;

    public Telemetry? LastTelemetry => _last;

    public Task StartAsync(CancellationToken ct = default)
    {
        _running = true;
        _ = Task.Run(async () =>
        {
            while (_running)
            {
                var heat = _heater/255.0*3.0;
                var cool = _fan/255.0*2.0;
                _t += heat - cool - 0.05;

                if (!_estop && _mode=="AUTO")
                {
                    if (_t < _sp-2) { _state="HEATING"; _heater=200; _fan=0; }
                    else if (_t > _sp+2) { _state="COOLING"; _heater=0; _fan=180; }
                    else { _state="IDLE"; _heater=0; _fan=0; }
                }

                if (_t >= _lcrit) { _state="CRITICAL"; _heater=0; _fan=255; _alarm="ON"; }
                else _alarm = "NONE";

                _last = new Telemetry(_t,_sp,_lmin,_lmax,_lcrit,_mode,_state,_heater,_fan,_alarm,_estop, _estop?200:0, 1500, DateTime.UtcNow);
                await Task.Delay(100, ct);
            }
        }, ct);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default) { _running=false; return Task.CompletedTask; }

    public Task<string> SendAsync(string cmd, CancellationToken ct = default)
    {
        var s = cmd.Trim().ToUpperInvariant();
        if (s=="GET;") return Task.FromResult("ACK;");
        if (s=="START;") { _estop=false; return Task.FromResult("ACK;"); }
        if (s=="STOP;") { _heater=0; _fan=255; return Task.FromResult("ACK;"); }
        if (s=="RST_ESTOP;") { _estop=false; return Task.FromResult("ACK;"); }
        if (s.StartsWith("MODE=")) { _mode = s.Contains("AUTO")?"AUTO":"MAN"; return Task.FromResult("ACK;"); }
        if (s.StartsWith("SET_SP=")) { _sp = double.Parse(s[7..].TrimEnd(';')); return Task.FromResult("ACK;"); }
        if (s.StartsWith("SET_LIMS=")) { var v=s[9..].TrimEnd(';').Split(','); _lmin=double.Parse(v[0]); _lmax=double.Parse(v[1]); _lcrit=double.Parse(v[2]); return Task.FromResult("ACK;"); }
        if (s.StartsWith("MAN=")) { var v=s[4..].TrimEnd(';').Split(','); _mode="MAN"; _heater=int.Parse(v[0]); _fan=int.Parse(v[1]); return Task.FromResult("ACK;"); }
        return Task.FromResult("ERR=CMD;");
    }
}
