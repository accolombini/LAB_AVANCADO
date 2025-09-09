using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using Forno.Api.Device;

namespace Forno.Api.Device;

/// <summary>
/// Comunicação Serial com Arduino do Forno Industrial - MOMENTO 1
/// Processa saída do terminal: "TEMP: 1500.0C | SP: 1500C | AQUECENDO | M:ON V:OFF A:OFF"
/// </summary>
public sealed class ArduinoDevice : IDevice, IDisposable
{
    private readonly SerialPort _port;
    private readonly StringBuilder _rxBuffer = new();
    private Telemetry? _lastTelemetry;
    private bool _isRunning;

    public event EventHandler<Telemetry>? TelemetryReceived;

    public ArduinoDevice(string portName, int baudRate = 115200)
    {
        _port = new SerialPort(portName, baudRate)
        {
            NewLine = "\n",
            ReadTimeout = 1000,
            WriteTimeout = 1000,
            Encoding = Encoding.UTF8
        };
        _port.DataReceived += OnSerialDataReceived;
    }

    public Telemetry? LastTelemetry => _lastTelemetry;

    public async Task StartAsync(CancellationToken ct = default)
    {
        try
        {
            if (!_port.IsOpen)
            {
                _port.Open();
                _isRunning = true;
                
                // Aguarda um pouco para estabilizar conexão
                await Task.Delay(2000, ct);
                
                Console.WriteLine($"✅ Conectado ao Arduino na porta {_port.PortName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao conectar Arduino: {ex.Message}");
            throw;
        }
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        try
        {
            _isRunning = false;
            if (_port.IsOpen)
            {
                _port.Close();
                Console.WriteLine("🔌 Desconectado do Arduino");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao desconectar: {ex.Message}");
        }
        return Task.CompletedTask;
    }

    public async Task<string> SendAsync(string command, CancellationToken ct = default)
    {
        try
        {
            if (!_port.IsOpen) return "ERRO: Porta não está aberta";
            
            await _port.BaseStream.WriteAsync(Encoding.UTF8.GetBytes(command + "\n"), ct);
            Console.WriteLine($"📤 Comando enviado: {command}");
            return "OK";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao enviar comando: {ex.Message}");
            return $"ERRO: {ex.Message}";
        }
    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (!_isRunning) return;
            
            string data = _port.ReadExisting();
            _rxBuffer.Append(data);
            
            // Processa linhas completas
            string buffer = _rxBuffer.ToString();
            var lines = buffer.Split('\n');
            
            // Mantém a última linha incompleta no buffer
            _rxBuffer.Clear();
            _rxBuffer.Append(lines[^1]);
            
            // Processa linhas completas
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    TryParseTelemetryLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao processar dados seriais: {ex.Message}");
        }
    }

    private void TryParseTelemetryLine(string line)
    {
        try
        {
            // Padrão: "TEMP: 1500.0C | SP: 1500C | AQUECENDO | M:ON V:OFF A:OFF"
            var tempMatch = Regex.Match(line, @"TEMP:\s*([\d.]+)C");
            var spMatch = Regex.Match(line, @"SP:\s*([\d.]+)C");
            var stateMatch = Regex.Match(line, @"\|\s*([A-Z\s]+)\s*\|");
            var macaricoMatch = Regex.Match(line, @"M:(ON|OFF)");
            var ventiladorMatch = Regex.Match(line, @"V:(ON|OFF)");
            var alarmeMatch = Regex.Match(line, @"A:(ON|OFF)");
            
            if (tempMatch.Success && spMatch.Success)
            {
                var temperatura = double.Parse(tempMatch.Groups[1].Value);
                var setpoint = double.Parse(spMatch.Groups[1].Value);
                var estado = stateMatch.Success ? stateMatch.Groups[1].Value.Trim() : "DESCONHECIDO";
                var macaricoOn = macaricoMatch.Success && macaricoMatch.Groups[1].Value == "ON";
                var ventiladorOn = ventiladorMatch.Success && ventiladorMatch.Groups[1].Value == "ON";
                var alarmeOn = alarmeMatch.Success && alarmeMatch.Groups[1].Value == "ON";
                
                // Detecta estados especiais
                bool interrupcaoCritica = line.Contains("INTERRUPCAO CRITICA");
                bool sistemaAtivo = !interrupcaoCritica;
                
                var telemetry = new Telemetry(
                    TemperaturaAtual: temperatura,
                    SetPoint: setpoint,
                    TemperaturaAlarme: 1600.0,      // Conforme MOMENTO 1
                    TemperaturaCritica: 1750.0,     // Conforme MOMENTO 1
                    MacaricoLigado: macaricoOn,
                    VentiladorLigado: ventiladorOn,
                    AlarmeAtivo: alarmeOn,
                    InterrupcaoCritica: interrupcaoCritica,
                    SistemaAtivo: sistemaAtivo,
                    Estado: estado,
                    Timestamp: DateTime.UtcNow
                );
                
                _lastTelemetry = telemetry;
                TelemetryReceived?.Invoke(this, telemetry);
                
                Console.WriteLine($"📊 Telemetria: {temperatura:F1}°C | {estado} | M:{(macaricoOn ? "ON" : "OFF")} V:{(ventiladorOn ? "ON" : "OFF")} A:{(alarmeOn ? "ON" : "OFF")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao interpretar linha: {line} - {ex.Message}");
        }
    }

    public void Dispose()
    {
        StopAsync().Wait();
        _port?.Dispose();
    }
}
