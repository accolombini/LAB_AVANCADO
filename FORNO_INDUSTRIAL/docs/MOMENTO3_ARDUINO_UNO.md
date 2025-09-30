# 🔥 FORNO INDUSTRIAL - MOMENTO 3
## Arduino UNO com Comunicação Serial - Integração Completa

> **Data:** 29 de setembro de 2025  
> **Objetivo:** Integrar Arduino UNO (MOMENTO 1) com API C# (MOMENTO 2) via Serial/USB para comunicação bidirecional

---

## 🎯 **OBJETIVOS DO MOMENTO 3**

### ✅ **Implementado:**
- ✅ Arduino UNO com recepção de comandos via Serial
- ✅ Comunicação Serial bidirecional UNO ↔ API  
- ✅ Setpoint configurável remotamente
- ✅ Comandos de emergência e reset
- ✅ API C# processando dados do Arduino
- ✅ Interface web SCADA em tempo real
- ✅ SignalR para dados ao vivo

---

## 🏗️ **ARQUITETURA CORRETA**

```
Arduino UNO (MOMENTO 1) ←─ Serial/USB ─→ API C# (MOMENTO 2) ←─ SignalR ─→ Interface SCADA (MOMENTO 3)
       ↑                                      ↑                              ↑
 Sensores/Atuadores              Processamento/Logs              Controle/Monitoramento
```

### **Fluxo de Dados:**
1. **Arduino UNO** → **API**: Telemetria via Serial (formato texto)
2. **API** → **Arduino UNO**: Comandos via Serial (SET_TEMP, EMERGENCY_STOP, RESET)
3. **API** → **Web UI**: Dados em tempo real via SignalR  
4. **Web UI** → **API**: Comandos do usuário via HTTP

---

## 📡 **COMANDOS SERIAL (API → Arduino)**

### **🎛️ Configurar Setpoint**
```
SET_TEMP=1550
```
**Resposta Arduino:**
```
>>> COMANDO RECEBIDO: SET_TEMP=1550
>>> SETPOINT ALTERADO PARA: 1550C
```

### **🚨 Parada de Emergência**
```
EMERGENCY_STOP
```
**Resposta Arduino:**
```
>>> COMANDO RECEBIDO: EMERGENCY_STOP
>>> PARADA DE EMERGENCIA ATIVADA!
>>> SISTEMA DESLIGADO - VENTILADOR FORCADO
```

### **🔄 Reset do Sistema**
```
RESET_SYSTEM
```
**Resposta Arduino:**
```
>>> COMANDO RECEBIDO: RESET_SYSTEM
>>> SISTEMA RESETADO!
>>> SETPOINT RESTAURADO: 1500C
```

---

## 📊 **TELEMETRIA (Arduino → API)**

### **Formato da Saída Serial:**
```
TEMP: 1545.0C | SP: 1500C | AQUECENDO (+5C)
MACARICO: LIGADO      | VENTILADOR: DESLIGADO   | ALARME: INATIVO
------------------------------------------------
```

### **Processamento pela API:**
- **Regex Parser**: Extrai temperatura, setpoint, estado, componentes
- **Telemetry Object**: Converte para formato JSON estruturado
- **SignalR Broadcast**: Transmite para interfaces conectadas

---

## ⚙️ **CONFIGURAÇÃO DO SISTEMA**

### **1. Arduino UNO - Compilar e Upload**
```bash
cd firmware/FornoLab_PIO
pio run -e uno
pio run -e uno -t upload
pio device monitor -e uno
```

### **2. API C# - Configurar Serial**
Edite `api/appsettings.json`:
```json
{
  "UseSimulator": false,
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

### **3. Executar Sistema Completo**
```bash
# Terminal 1: API
cd api
dotnet run

# Terminal 2: Interface Web  
cd ui
dotnet run

# Terminal 3: Monitor Arduino (opcional)
cd firmware/FornoLab_PIO
pio device monitor -e uno
```

---

## 🚀 **PROCEDIMENTO DE TESTE**

### **1. URLs de Acesso**
- **API**: http://localhost:5002
- **Interface SCADA**: http://localhost:5001
- **Health Check**: http://localhost:5002/health

### **2. Teste Manual via API**
```bash
# Verificar API
curl http://localhost:5002/health

# Verificar telemetria
curl http://localhost:5002/api/telemetry

# Alterar setpoint
curl -X POST http://localhost:5002/api/setpoint \
  -H "Content-Type: application/json" \
  -d '{"temperature": 1600}'

# Parada de emergência
curl -X POST http://localhost:5002/api/emergency-stop

# Reset do sistema
curl -X POST http://localhost:5002/api/reset
```

### **3. Interface Web SCADA**
- Abra http://localhost:5001
- Monitore gráficos em tempo real
- Use controles para alterar setpoint
- Teste botões de emergência e reset

---

## 🔄 **PINOS ARDUINO UNO**

| Componente | Pino | Função |
|------------|------|---------|
| Maçarico | GPIO 5 | PWM Output (Aquecimento) |
| Ventilador | GPIO 6 | PWM Output (Resfriamento) |
| Alarme LED | GPIO 3 | Digital Output (Indicador) |

---

## 📊 **LOGS E MONITORAMENTO**

### **Console Arduino UNO:**
```
=== FORNO INDUSTRIAL - MOMENTO 1 ===
Temperatura: 1000-1800C
Regime: 1500C
Alarme: 1600C
Interrupcao: 1750C
Comandos via Serial:
  SET_TEMP=1550  - Alterar setpoint
  EMERGENCY_STOP - Parada de emergencia
  RESET_SYSTEM   - Reset do sistema
Sistema iniciado!

TEMP: 1545.0C | SP: 1500C | AQUECENDO (+5C)
MACARICO: LIGADO      | VENTILADOR: DESLIGADO   | ALARME: INATIVO
------------------------------------------------
```

### **Console API:**
```
🚀 API do Forno Industrial - MOMENTO 2
✅ Conectado ao Arduino na porta /dev/tty.usbmodem1101
📡 SignalR Hub: /hubs/forno
📊 Telemetria: 1545.0°C | AQUECENDO | M:ON V:OFF A:OFF
📤 Comando enviado: SET_TEMP=1600
```

---

## 🛠️ **RESOLUÇÃO DE PROBLEMAS**

### **Arduino não detectado:**
```bash
# Listar portas disponíveis
pio device list

# Verificar conexão USB
ls /dev/tty.usb*     # macOS
ls /dev/ttyUSB*      # Linux  
```

### **API não conecta:**
- Verificar porta serial em `appsettings.json`
- Verificar se Arduino está conectado e funcionando
- Verificar permissões de acesso à porta serial

### **Interface não atualiza:**
- Verificar se API está recebendo dados do Arduino
- Verificar console do navegador (F12) para erros SignalR
- Recarregar página da interface

### **Comandos não funcionam:**
- Verificar se `UseSimulator: false` em appsettings.json
- Verificar logs da API para confirmação de envio
- Verificar monitor serial do Arduino para recepção

---

## ✅ **FUNCIONALIDADES IMPLEMENTADAS**

### ✅ **Controle de Temperatura (Arduino)**
- Aquecimento automático até temperatura crítica (1750°C)
- Setpoint configurável via comando serial
- Alarme aos 1600°C  
- Interrupção crítica aos 1750°C

### ✅ **Comunicação Bidirecional**
- **Arduino → API**: Telemetria contínua via serial
- **API → Arduino**: Comandos de controle via serial
- **API → Interface**: Dados em tempo real via SignalR
- **Interface → API**: Controles via HTTP

### ✅ **Segurança Industrial**
- Parada de emergência remota
- Reset remoto do sistema
- Monitoramento contínuo de temperatura
- Alarmes visuais e de sistema

### ✅ **Interface SCADA**
- Gráficos em tempo real
- Controles visuais intuitivos
- Indicadores de alarme
- Histórico de dados

---

## 🎯 **MOMENTO 3 CONCLUÍDO**

**🟢 ARQUITETURA CORRETA IMPLEMENTADA**
```
Arduino UNO ←→ API C# ←→ Interface Web SCADA
```

✅ **Sistema industrial didático completo:**
- Hardware Arduino UNO (MOMENTO 1) ✓
- API C# de intermediação (MOMENTO 2) ✓  
- Interface web SCADA (MOMENTO 3) ✓
- Comunicação serial bidirecional ✓
- Controles remotos funcionais ✓

---

## 🚀 **PROCEDIMENTO FINAL DE DEMONSTRAÇÃO**

### **Passo a Passo Completo:**

1. **Conectar Arduino UNO** ao computador via USB

2. **Compilar e fazer upload** do firmware:
   ```bash
   cd firmware/FornoLab_PIO
   pio run -e uno -t upload
   ```

3. **Configurar API** para Arduino real:
   - Editar `api/appsettings.json`: `"UseSimulator": false`
   - Verificar porta serial correta

4. **Iniciar sistema completo** em 3 terminais:
   ```bash
   # Terminal 1
   cd api && dotnet run
   
   # Terminal 2  
   cd ui && dotnet run
   
   # Terminal 3 (monitor)
   cd firmware/FornoLab_PIO && pio device monitor -e uno
   ```

5. **Testar funcionalidades**:
   - Abrir http://localhost:5001 (Interface SCADA)
   - Verificar gráficos em tempo real
   - Alterar setpoint via interface
   - Testar parada de emergência
   - Verificar logs no terminal Arduino

---

**✨ Sistema pronto para demonstração didática! ✨**