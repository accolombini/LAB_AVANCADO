namespace Forno.Api.Device;
public sealed class LastLogService{ public string? Command{get;private set;} public string? Response{get;private set;} public DateTime? TimestampUtc{get;private set;} public void Set(string c,string r){ Command=c; Response=r; TimestampUtc=DateTime.UtcNow; } }
