namespace Forno.Ui.Models;

/// <summary>
/// Modelo de configuração do Forno Industrial
/// Espelha a estrutura da API para sincronização
/// </summary>
public class FornoConfiguration
{
    public bool UseSimulator { get; set; }
    public string SerialPort { get; set; } = "/dev/tty.usbmodem1101";
    public FornoIndustrialSettings FornoIndustrial { get; set; } = new();
}

/// <summary>
/// Configurações específicas do forno industrial
/// </summary>
public class FornoIndustrialSettings
{
    public double TemperaturaMinima { get; set; } = 1000.0;
    public double TemperaturaMaxima { get; set; } = 1800.0;
    public double TemperaturaSetpoint { get; set; } = 1500.0;
    public double TemperaturaAlarme { get; set; } = 1600.0;
    public double TemperaturaCritica { get; set; } = 1750.0;
}

/// <summary>
/// Request para atualização de configuração
/// </summary>
public class ConfigurationUpdateRequest
{
    public bool? UseSimulator { get; set; }
    public string? SerialPort { get; set; }
    public FornoIndustrialSettings? FornoIndustrial { get; set; }
}

/// <summary>
/// Resultado da validação de configuração
/// </summary>
public class ValidationResult
{
    public bool Valid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}