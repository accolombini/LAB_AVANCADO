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

**✨ Sistema pronto para demonstração didática! ✨**

---

## 🎯 **OBJETIVOS DO MOMENTO 3**

### ✅ **Implementado:**
- ✅ Firmware ESP32 com WiFi integrado
- ✅ Comunicação HTTP Client → API C#
- ✅ Comunicação HTTP Server ← API C#
- ✅ Envio de telemetria em tempo real
- ✅ Recepção de comandos (setpoint, emergência)
- ✅ Simulação térmica avançada
- ✅ Interface web SCADA funcional

---

## 🏗️ **ARQUITETURA COMPLETA**

```
ESP32 (Hardware) ←─ WiFi/HTTP ─→ API C# (.NET) ←─ SignalR ─→ Interface Web SCADA
     ↑                              ↑                           ↑
 Sensores/Atuadores          Processamento/Logs          Controle/Monitoramento
```

### **Fluxo de Dados:**
1. **ESP32** → **API**: Telemetria a cada 2 segundos via HTTP POST
2. **API** → **ESP32**: Comandos via HTTP POST (setpoint, emergência)
3. **API** → **Web UI**: Dados em tempo real via SignalR
4. **Web UI** → **API**: Comandos do usuário via HTTP

---

## 📱 **ENDPOINTS DO ESP32**

### **🌡️ Status do Sistema**
```http
GET http://[IP_ESP32]/status
```
**Resposta:**
```json
{
  "temperatura_atual": 1520.5,
  "setpoint": 1500.0,
  "sistema_ativo": true,
  "alarme_ativo": false,
  "interrupcao_critica": false,
  "macarico_ligado": true,
  "ventilador_ligado": false
}
```

### **🎛️ Configurar Setpoint**
```http
POST http://[IP_ESP32]/setpoint
Content-Type: application/x-www-form-urlencoded

temperature=1550
```

### **🚨 Parada de Emergência**
```http
POST http://[IP_ESP32]/emergency-stop
```

### **🔄 Reset do Sistema**
```http
POST http://[IP_ESP32]/reset
```

---

## 📡 **COMUNICAÇÃO COM API**

### **Envio de Telemetria (ESP32 → API)**
```http
POST http://[IP_API]:5002/api/telemetry
Content-Type: application/json

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
  "timestamp": "2025-09-29T12345Z"
}
```

### **Frequência de Envio:**
- **Telemetria**: A cada 2 segundos
- **Comandos**: Sob demanda via HTTP

---

## ⚙️ **CONFIGURAÇÃO**

### **1. Configurar WiFi no ESP32**
Edite em `src/esp32/FornoLab.cpp`:
```cpp
const char* WIFI_SSID = "SUA_REDE_WIFI";
const char* WIFI_PASSWORD = "SUA_SENHA_WIFI";
const char* API_HOST = "http://192.168.1.100";  // IP da máquina com a API
```

### **2. Configurar API para ESP32**
Edite em `api/appsettings.json`:
```json
{
  "UseSimulator": false,
  "SerialPort": "",
  "ESP32_IP": "192.168.1.200",
  "ESP32_Enabled": true
}
```

### **3. Compilar e Upload**
```bash
cd firmware/FornoLab_PIO
pio run -e esp32
pio run -e esp32 -t upload
pio device monitor -e esp32
```

---

## 🔄 **PINOS ESP32**

| Componente | Pino | Função |
|------------|------|---------|
| Maçarico | GPIO 5 | PWM Output (Aquecimento) |
| Ventilador | GPIO 6 | PWM Output (Resfriamento) |
| Alarme LED | GPIO 2 | Digital Output (Indicador) |

---

## 🚀 **PROCEDIMENTO DE TESTE**

### **1. Iniciar Sistema Completo**
```bash
# Terminal 1: API
cd api
dotnet run

# Terminal 2: Interface Web
cd ui  
dotnet run

# Terminal 3: Monitor ESP32
cd firmware/FornoLab_PIO
pio device monitor -e esp32
```

### **2. URLs de Acesso**
- **API**: http://localhost:5002
- **Interface SCADA**: http://localhost:5001  
- **ESP32**: http://[IP_DO_ESP32]

### **3. Teste de Comunicação**
```bash
# Verificar API
curl http://localhost:5002/health

# Verificar ESP32
curl http://[IP_ESP32]/status

# Alterar setpoint via ESP32
curl -X POST http://[IP_ESP32]/setpoint -d "temperature=1550"

# Alterar setpoint via API
curl -X POST http://localhost:5002/api/setpoint \
  -H "Content-Type: application/json" \
  -d '{"temperature": 1600}'
```

---

## 📊 **LOGS E MONITORAMENTO**

### **Console ESP32:**
```
=== FORNO INDUSTRIAL ESP32 - MOMENTO 3 ===
Conectando ao WiFi...
WiFi conectado!
IP ESP32: 192.168.1.200
API URL: http://192.168.1.100:5002
Servidor web iniciado na porta 80
Sistema iniciado!

TEMP: 1545.0C | SP: 1500C | AQUECENDO (+5C)
MACARICO: LIGADO      | VENTILADOR: DESLIGADO   | ALARME: INATIVO
📡 Telemetria enviada: 200
------------------------------------------------
```

### **Console API:**
```
🚀 API do Forno Industrial - MOMENTO 2
🔥 Configurado para usar ESP32 Real
📡 SignalR Hub: /hubs/forno
📊 Telemetria recebida do ESP32: 1545.0°C | AQUECENDO
```

---

## 🛠️ **RESOLUÇÃO DE PROBLEMAS**

### **WiFi não conecta:**
- Verificar SSID e senha em `FornoLab.cpp`
- Verificar se ESP32 está no alcance do WiFi
- Verificar se a rede aceita dispositivos IoT

### **API não recebe dados:**
- Verificar IP da API em `API_HOST`
- Verificar se API está rodando na porta 5002
- Verificar firewall/antivírus

### **ESP32 não responde:**
- Verificar se está conectado ao WiFi
- Ping para o IP do ESP32
- Verificar logs no monitor serial

### **Interface não atualiza:**
- Verificar se SignalR está funcionando
- Verificar console do navegador (F12)
- Recarregar página da interface

---

## 📈 **FUNCIONALIDADES IMPLEMENTADAS**

### ✅ **Controle de Temperatura**
- Aquecimento automático quando T < Setpoint
- Resfriamento quando T > Setpoint + 50°C
- Manutenção quando próximo do setpoint

### ✅ **Segurança**
- Alarme aos 1600°C
- Interrupção crítica aos 1750°C
- Parada de emergência remota
- Reset remoto do sistema

### ✅ **Comunicação**
- Telemetria em tempo real (2s)
- Comandos bidirecionais HTTP
- Status via API REST
- Monitoramento via Serial

### ✅ **Interface SCADA**
- Gráficos em tempo real
- Controles visuais
- Indicadores de alarme
- Histórico de dados

---

## 🎯 **MOMENTO 3 COMPLETO**

**🟢 CONCLUÍDO** - Sistema industrial completo com:
- Hardware ESP32 conectado
- Comunicação WiFi/HTTP bidirecional  
- API C# de intermediação
- Interface web SCADA funcional
- Telemetria em tempo real
- Controles remotos operacionais

---

## 🔧 **PRÓXIMAS MELHORIAS**

1. **Banco de dados** para histórico
2. **Autenticação** na interface web
3. **Múltiplos fornos** conectados
4. **Relatórios** automáticos
5. **Integração** com sistemas ERP

---

**✨ Sistema pronto para demonstração e uso didático! ✨**