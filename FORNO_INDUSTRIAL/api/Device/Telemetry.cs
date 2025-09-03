namespace Forno.Api.Device;
public record Telemetry(double T,double SP,double LimMin,double LimMax,double LimCrit,string Mode,string State,int Heater,int Fan,string Alarm,bool EStop,long DtEstopUs,long LoopUs,DateTime Timestamp);
