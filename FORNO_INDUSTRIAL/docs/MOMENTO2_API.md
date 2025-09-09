# 🔥 FORNO INDUSTRIAL - MOMENTO 2
## API de Captura e Monitoramento

> **Data:** 9 de setembro de 2025  
> **Objetivo:** Criar API RESTful para comunicação com Arduino e transmissão de dados em tempo real

---

## 🎯 **OBJETIVOS DO MOMENTO 2**

### ✅ **Implementado:**
- ✅ API RESTful em C# .NET 9.0
- ✅ Comunicação Serial com Arduino UNO
- ✅ Simulador integrado para testes
- ✅ SignalR para dados em tempo real
- ✅ Endpoints especializados para SCADA
- ✅ Sistema de logs e monitoramento
- ✅ Configuração flexível (Real/Simulador)

---

## 🔌 **ARQUITETURA TÉCNICA**

### **Componentes Principais:**
```
Arduino UNO (MOMENTO 1) ←─ Serial/USB ─→ API C# (MOMENTO 2)
                                              ↓
                                         SignalR Hub
                                              ↓
                                      Dashboard Web (MOMENTO 3)
```

### **Tecnologias Utilizadas:**
- **Backend:** C# .NET 9.0, ASP.NET Core
- **Comunicação:** System.IO.Ports (Serial)
- **Tempo Real:** SignalR
- **Configuração:** appsettings.json
- **Logging:** Microsoft.Extensions.Logging

---

## 📡 **ENDPOINTS DA API**

### **🏥 Health Check**
```http
GET /health
```
**Resposta:**
```json
{
  "status": "Healthy",
  "simulator": true,
  "lastTelemetryUtc": "2025-09-09T14:30:00Z",
  "momento": "MOMENTO 2 - API Industrial"
}
```

### **📊 Telemetria Atual**
```http
GET /api/telemetry
```
**Resposta:**
```json
{
  "temperaturaAtual": 1520.5,
  "setPoint": 1500.0,
  "temperaturaAlarme": 1600.0,
  "temperaturaCritica": 1750.0,
  "macaricoLigado": true,
  "ventiladorLigado": false,
  "alarmeAtivo": false,
  "interrupcaoCritica": false,
  "sistemaAtivo": true,
  "estado": "AQUECENDO",
  "timestamp": "2025-09-09T14:30:00Z"
}
```

### **📈 Dashboard Formatado**
```http
GET /api/dashboard
```
**Resposta:**
```json
{
  "temperatura": {
    "atual": 1520.5,
    "setpoint": 1500.0,
    "alarme": 1600.0,
    "critica": 1750.0
  },
  "atuadores": {
    "macarico": true,
    "ventilador": false,
    "alarme": false
  },
  "sistema": {
    "ativo": true,
    "interrupcaoCritica": false,
    "estado": "AQUECENDO"
  },
  "timestamp": "2025-09-09T14:30:00Z"
}
```

### **🎛️ Controle de Setpoint**
```http
POST /api/setpoint
Content-Type: application/json

{
  "temperature": 1550.0
}
```

### **🚨 Parada de Emergência**
```http
POST /api/emergency-stop
```

### **🔄 Reset do Sistema**
```http
POST /api/reset
```

---

## 🔄 **SIGNALR - TEMPO REAL**

### **Hub:** `/hubs/forno`

### **Eventos do Cliente:**
- `JoinMonitoring()` - Conectar ao monitoramento
- `LeaveMonitoring()` - Sair do monitoramento
- `SendCommand(command)` - Enviar comando

### **Eventos do Servidor:**
- `TelemetryUpdate(telemetry)` - Nova telemetria
- `Connected(message)` - Confirmação de conexão
- `CommandSent(command)` - Comando enviado

---

## ⚙️ **CONFIGURAÇÃO**

### **appsettings.json:**
```json
{
  "UseSimulator": true,
  "SerialPort": "/dev/tty.usbmodem1101",
  "FornoIndustrial": {
    "TemperaturaMinima": 1000.0,
    "TemperaturaMaxima": 1800.0,
    "TemperaturaSetpoint": 1500.0,
    "TemperaturaAlarme": 1600.0,
    "TemperaturaCritica": 1750.0
  }
}
```

### **Modos de Operação:**
1. **Simulador** (`UseSimulator: true`)
   - Para desenvolvimento e testes
   - Simula comportamento do MOMENTO 1
   - Não requer Arduino físico

2. **Arduino Real** (`UseSimulator: false`)
   - Conecta via porta serial
   - Processa saída do terminal Arduino
   - Formato: `"TEMP: 1500.0C | SP: 1500C | AQUECENDO | M:ON V:OFF A:OFF"`

---

## 🚀 **COMO EXECUTAR**

### **1. Compilar:**
```bash
cd api
dotnet build
```

### **2. Executar:**
```bash
dotnet run
```

### **3. Testar:**
- Health Check: http://localhost:5000/health
- Dashboard: http://localhost:5000/api/dashboard
- SignalR: ws://localhost:5000/hubs/forno

---

## 🔍 **LOGS E MONITORAMENTO**

### **Console Output:**
```
🚀 API do Forno Industrial - MOMENTO 2
🔥 Configurado para usar SIMULADOR do Forno Industrial
📡 SignalR Hub: /hubs/forno
🔗 Health Check: /health
📊 Dashboard: /api/dashboard
```

### **Telemetria em Tempo Real:**
```
📊 Telemetria: 1520.5°C | AQUECENDO | M:ON V:OFF A:OFF
⚠️ *** ALARME ATIVADO! ***
🚨 *** INTERRUPÇÃO CRÍTICA ATIVADA! ***
```

---

## 📝 **PRÓXIMOS PASSOS - MOMENTO 3**

1. **Interface Web SCADA**
2. **Gráficos em tempo real**
3. **Controles visuais**
4. **Histórico de dados**
5. **Relatórios operacionais**

---

## ✅ **STATUS MOMENTO 2**
**🟢 CONCLUÍDO** - API funcional com simulador e comunicação serial
