using Forno.Api.Device;

namespace Forno.Api.Device;

/// <summary>
/// Simulador do Forno Industrial - MOMENTO 1
/// Simula comportamento do Arduino com temperaturas 1000-1800°C
/// </summary>
public sealed class SimulatedDevice : IDevice
{
    private bool _isRunning;
    private Telemetry? _lastTelemetry;
    private CancellationTokenSource? _cts;

    // Parâmetros do MOMENTO 1
    private double _temperaturaAtual = 1450.0;     // Temperatura inicial próxima ao setpoint
    private double _setpoint = 1500.0;             // Temperatura de regime
    private readonly double _tempAlarme = 1600.0;  // Alarme crítico
    private readonly double _tempCritica = 1750.0; // Interrupção automática
    
    // Estados do sistema
    private bool _macaricoLigado = false;
    private bool _ventiladorLigado = false;
    private bool _alarmeAtivo = false;
    private bool _interrupcaoCritica = false;
    private bool _sistemaAtivo = true;

    // Parâmetros de simulação
    private readonly double _incrementoTemp = 5.0;  // °C por ciclo (aquecimento)
    private readonly double _decrementoTemp = 8.0;  // °C por ciclo (resfriamento)
    private readonly double _perdaAmbiente = 2.0;   // °C por ciclo (perdas naturais)

    public event EventHandler<Telemetry>? TelemetryReceived;
    public Telemetry? LastTelemetry => _lastTelemetry;

    public Task StartAsync(CancellationToken ct = default)
    {
        if (_isRunning) return Task.CompletedTask;
        
        _isRunning = true;
        _cts = new CancellationTokenSource();
        
        Console.WriteLine("🔥 Simulador do Forno Industrial iniciado");
        Console.WriteLine("📊 Temperatura: 1000-1800°C | Setpoint: 1500°C");
        Console.WriteLine("⚠️  Alarme: 1600°C | 🚨 Crítico: 1750°C");
        
        // Inicia simulação em background
        _ = Task.Run(async () => await SimulationLoop(_cts.Token), ct);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _isRunning = false;
        _cts?.Cancel();
        Console.WriteLine("🛑 Simulador do Forno Industrial parado");
        return Task.CompletedTask;
    }

    public Task<string> SendAsync(string command, CancellationToken ct = default)
    {
        var cmd = command.Trim().ToUpperInvariant();
        
        Console.WriteLine($"📤 Comando recebido: {command}");
        
        return cmd switch
        {
            "GET_STATUS" => Task.FromResult("SIMULADOR_ATIVO"),
            "RESET_SYSTEM" => ResetSystem(),
            "SET_TEMP" when command.Contains("=") => SetTemperature(command),
            "EMERGENCY_STOP" => EmergencyStop(),
            _ => Task.FromResult("OK")
        };
    }

    private async Task SimulationLoop(CancellationToken ct)
    {
        while (_isRunning && !ct.IsCancellationRequested)
        {
            try
            {
                // 1. Verificar temperatura crítica (PRIORIDADE MÁXIMA)
                VerificarTemperaturaCritica();
                
                // 2. Controlar temperatura
                ControlarTemperatura();
                
                // 3. Simular planta térmica (inclui exibição de status)
                SimularTemperatura();
                
                // 4. Gerar telemetria
                GerarTelemetria();
                
                // Aguardar próximo ciclo (1 segundo como no Arduino)
                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erro na simulação: {ex.Message}");
            }
        }
    }

    private void VerificarTemperaturaCritica()
    {
        // INTERRUPÇÃO CRÍTICA aos 1750°C
        if (_temperaturaAtual >= _tempCritica && !_interrupcaoCritica)
        {
            _interrupcaoCritica = true;
            _sistemaAtivo = false;
            Console.WriteLine("🚨 *** INTERRUPÇÃO CRÍTICA ATIVADA! TEMP >= 1750°C ***");
            Console.WriteLine("🚨 *** VENTILADORES EM FORÇA TOTAL ***");
        }
        
        // RESETAR INTERRUPÇÃO quando temperatura voltar ao setpoint (1500°C)
        if (_interrupcaoCritica && _temperaturaAtual <= _setpoint)
        {
            _interrupcaoCritica = false;
            _sistemaAtivo = true;
            _alarmeAtivo = false;
            Console.WriteLine("✅ *** SISTEMA REATIVADO - TEMP <= 1500°C ***");
            Console.WriteLine("✅ *** REINICIANDO CICLO DE AQUECIMENTO ***");
        }
        
        // ALARME aos 1600°C (só se não estiver em interrupção)
        if (!_interrupcaoCritica && _temperaturaAtual >= _tempAlarme && !_alarmeAtivo)
        {
            _alarmeAtivo = true;
            Console.WriteLine("⚠️ *** ALARME! TEMPERATURA CRÍTICA >= 1600°C ***");
        }
        
        // Desligar alarme se temperatura baixou para < 1600°C (e não está em interrupção)
        if (!_interrupcaoCritica && _temperaturaAtual < _tempAlarme && _alarmeAtivo)
        {
            _alarmeAtivo = false;
            Console.WriteLine("✅ *** ALARME DESATIVADO - TEMP < 1600°C ***");
        }
        
        // Manter alarme durante interrupção crítica
        if (_interrupcaoCritica)
        {
            _alarmeAtivo = true;
        }
    }

    private void ControlarTemperatura()
    {
        // Se em interrupção crítica, só ventilador força total
        if (_interrupcaoCritica)
        {
            _macaricoLigado = false;
            _ventiladorLigado = true;
            return;
        }
        
        // LÓGICA DO MOMENTO 1: SEMPRE AQUECE até atingir temperatura crítica
        // Não usa controle bang-bang tradicional - simula o ciclo didático completo
        if (_sistemaAtivo)
        {
            // CONTINUA AQUECENDO até atingir 1750°C (temperatura crítica)
            _macaricoLigado = true;
            _ventiladorLigado = false;
            
            // Estado baseado na situação atual
            if (_alarmeAtivo && !_interrupcaoCritica)
            {
                // Aquecendo em estado de alarme
            }
            else
            {
                // Aquecendo normal
            }
        }
        else
        {
            // Sistema inativo - desligar tudo
            _macaricoLigado = false;
            _ventiladorLigado = false;
        }
    }

    private void SimularTemperatura()
    {
        // SIMULAÇÃO EXATA DO MOMENTO 1
        
        // Aplicar efeitos na temperatura
        if (_macaricoLigado)
        {
            _temperaturaAtual += _incrementoTemp; // +5°C por ciclo
        }
        
        if (_ventiladorLigado)
        {
            _temperaturaAtual -= _decrementoTemp; // -8°C por ciclo
        }
        
        // Perdas naturais para o ambiente (sempre presente quando não aquecendo)
        if (!_macaricoLigado && _temperaturaAtual > 25.0)
        {
            _temperaturaAtual -= _perdaAmbiente; // -2°C por ciclo
        }
        
        // Limites físicos
        if (_temperaturaAtual < 25.0) _temperaturaAtual = 25.0;
        if (_temperaturaAtual > 1800.0) _temperaturaAtual = 1800.0;
        
        // Exibir status simples (sem duplicar com telemetria)
        ExibirStatusSimples();
    }

    private void ExibirStatusSimples()
    {
        // Status simplificado para evitar duplicação com telemetria
        string acao = "";
        if (_interrupcaoCritica)
        {
            acao = "INTERRUPCAO CRITICA";
        }
        else if (_macaricoLigado && _alarmeAtivo)
        {
            acao = "ALARME - AQUECENDO";
        }
        else if (_macaricoLigado)
        {
            acao = "AQUECENDO";
        }
        else if (_ventiladorLigado)
        {
            acao = "RESFRIANDO";
        }
        else
        {
            acao = "MANTENDO";
        }
        
        Console.WriteLine($"TEMP: {_temperaturaAtual:F1}°C | {acao} | M:{(_macaricoLigado ? "ON" : "OFF")} V:{(_ventiladorLigado ? "ON" : "OFF")} A:{(_alarmeAtivo ? "ON" : "OFF")}");
    }

    private void GerarTelemetria()
    {
        // Estado mais descritivo baseado na situação atual
        string estadoAtual;
        
        if (_interrupcaoCritica)
        {
            estadoAtual = "INTERRUPCAO CRITICA";
        }
        else if (_alarmeAtivo && _macaricoLigado)
        {
            estadoAtual = "ALARME - AQUECENDO";
        }
        else if (_macaricoLigado)
        {
            estadoAtual = "AQUECENDO";
        }
        else if (_ventiladorLigado)
        {
            estadoAtual = "RESFRIANDO";
        }
        else if (!_sistemaAtivo)
        {
            estadoAtual = "INATIVO";
        }
        else
        {
            estadoAtual = "MANTENDO";
        }
        
        var telemetry = new Telemetry(
            TemperaturaAtual: _temperaturaAtual,
            SetPoint: _setpoint,
            TemperaturaAlarme: _tempAlarme,
            TemperaturaCritica: _tempCritica,
            MacaricoLigado: _macaricoLigado,
            VentiladorLigado: _ventiladorLigado,
            AlarmeAtivo: _alarmeAtivo,
            InterrupcaoCritica: _interrupcaoCritica,
            SistemaAtivo: _sistemaAtivo,
            Estado: estadoAtual,
            Timestamp: DateTime.UtcNow
        );
        
        _lastTelemetry = telemetry;
        TelemetryReceived?.Invoke(this, telemetry);
    }

    private Task<string> ResetSystem()
    {
        _interrupcaoCritica = false;
        _sistemaAtivo = true;
        _alarmeAtivo = false;
        _temperaturaAtual = 1450.0;
        Console.WriteLine("🔄 Sistema resetado");
        return Task.FromResult("RESET_OK");
    }

    private Task<string> SetTemperature(string command)
    {
        try
        {
            var parts = command.Split('=');
            if (parts.Length == 2 && double.TryParse(parts[1], out double temp))
            {
                _setpoint = Math.Clamp(temp, 1000.0, 1800.0);
                Console.WriteLine($"🎯 Setpoint alterado para: {_setpoint:F1}°C");
                return Task.FromResult("SETPOINT_OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao alterar setpoint: {ex.Message}");
        }
        return Task.FromResult("SETPOINT_ERROR");
    }

    private Task<string> EmergencyStop()
    {
        _interrupcaoCritica = true;
        _sistemaAtivo = false;
        _macaricoLigado = false;
        _ventiladorLigado = true;
        _alarmeAtivo = true;
        Console.WriteLine("🛑 PARADA DE EMERGÊNCIA ATIVADA!");
        return Task.FromResult("EMERGENCY_STOP_OK");
    }
}
