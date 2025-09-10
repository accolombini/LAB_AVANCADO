# 🔥 Tutorial de Execução - SCADA Forno Industrial

## 📋 Visão Geral

Este tutorial explica como executar e operar o sistema SCADA do Forno Industrial, um sistema completo de monitoramento e controle em tempo real desenvolvido com:

- **API .NET 9**: Backend com simulador de forno e comunicação SignalR
- **Blazor Server**: Frontend moderno com dashboard interativo
- **SignalR**: Comunicação em tempo real para telemetria
- **Simulador Avançado**: Forno virtual com comportamento realístico

---

## 🚀 Iniciando o Sistema

### Pré-requisitos
- **.NET 9 SDK** instalado
- **Portas 5001 e 5002** livres (evitando conflito com AirPlay na porta 5000)
- **Terminal/Prompt** de comando

### 1. Iniciar a API (Backend)

```bash
# Navegar para o diretório da API
cd api

# Executar a API
dotnet run
```

**Saída esperada:**
```
🔥 Configurado para usar SIMULADOR do Forno Industrial
🚀 API do Forno Industrial - MOMENTO 2
📡 SignalR Hub: /hubs/forno
🔗 Health Check: /health
📊 Dashboard: /api/dashboard
info: DeviceHost[0]
      🔥 Iniciando comunicação com dispositivo...
🔥 Simulador do Forno Industrial iniciado
📊 Temperatura: 1000-1800°C | Setpoint: 1500°C
⚠️  Alarme: 1600°C | 🚨 Crítico: 1750°C
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5002
TEMP: 1455.0°C | AQUECENDO | M:ON V:OFF A:OFF
```

### 2. Iniciar a UI (Frontend) - **EM OUTRO TERMINAL**

```bash
# Navegar para o diretório da UI
cd ui

# Executar a interface
dotnet run
```

**Saída esperada:**
```
🎯 MOMENTO 3 - SCADA Dashboard iniciado
🔗 Interface: http://localhost:5001
📡 Conectando à API: http://localhost:5002
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
info: Forno.Ui.Services.FornoSignalRService[0]
      🔗 Conectado ao SignalR Hub do Forno
```

### 3. Acessar o Dashboard

Abra o navegador em: **http://localhost:5001**

---

## 🎛️ Operando o Dashboard

### Interface Principal

O dashboard possui 4 seções principais:

#### 🌡️ **1. Temperatura Principal**
- **Gauge em tempo real** mostrando temperatura atual
- **Indicador visual** com cores:
  - 🟢 **Verde**: Normal (< 1600°C)
  - 🟡 **Amarelo**: Alerta (1600-1750°C)
  - 🔴 **Vermelho**: Crítico (> 1750°C)
- **Setpoint**: Meta de temperatura (1500°C)

#### ⚙️ **2. Status dos Atuadores**
- **LIGADO**: Macarico ativo (aquecimento)
- **DESLIGADO**: Ventilador e alarme em repouso
- **INATIVO**: Estados de segurança

#### 🎮 **3. Controles**
- **Campo Setpoint**: Definir nova temperatura alvo
- **Botão Aplicar**: Enviar novo setpoint para o forno
- **Reset Sistema**: Reinicializar parâmetros
- **Parada Emergência**: Interrupção crítica total

#### 📊 **4. Status do Sistema**
- **Conexão**: Online/Offline
- **Estado**: AQUECENDO/RESFRIANDO/PARADO
- **Timestamp**: Última atualização

---

## 🔧 Comandos de Controle

### Alterar Setpoint
1. Digite nova temperatura no campo **Setpoint (°C)**
2. Clique em **✓ Aplicar**
3. Sistema ajustará automaticamente

### Parada de Emergência
1. Clique no botão **🔺 PARADA EMERGÊNCIA**
2. Sistema interrompe imediatamente
3. Requer reset manual para reativar

### Reset do Sistema
1. Clique em **🔄 Reset Sistema**
2. Parâmetros voltam ao padrão
3. Operação normal é retomada

---

## 🚨 Sistema de Alarmes

### Níveis de Alerta

#### ⚠️ **Alerta (1600°C)**
- Indicação visual amarela
- Sistema permanece operacional
- Monitoramento intensificado

#### 🚨 **Crítico (1750°C)**
- Banner vermelho de **INTERRUPÇÃO CRÍTICA**
- Sistema força resfriamento
- Parada automática de aquecimento

### Ações Automáticas
- **< 1600°C**: Operação normal
- **1600-1750°C**: Alerta ativo, aquecimento reduzido
- **> 1750°C**: Parada crítica, resfriamento forçado

---

## 📈 Monitoramento em Tempo Real

### Dados Dinâmicos
- **Atualização**: A cada 1 segundo
- **Telemetria**: Temperatura, estado, atuadores
- **Gráficos**: Tendências e histórico
- **Logs**: Eventos e comandos

### Indicadores Visuais
- **Pulse**: Animações nos componentes ativos
- **Cores**: Status baseado em temperatura
- **Badges**: Estados dos equipamentos
- **Timeline**: Histórico de eventos

---

## 🔍 Solução de Problemas

### Problema: Dashboard "Offline"
**Causa**: SignalR não conectado
**Solução**:
1. Verificar se API está rodando (porta 5002)
2. Reiniciar UI se necessário
3. Aguardar reconexão automática

### Problema: Dados não atualizam
**Causa**: Comunicação interrompida
**Solução**:
1. Verificar logs da API
2. Recarregar página (F5)
3. Verificar conectividade de rede

### Problema: Comandos não respondem
**Causa**: API não disponível
**Solução**:
1. Confirmar API ativa
2. Verificar logs de erro
3. Reiniciar serviços se necessário

---

## 🏁 Encerrando o Sistema

### Parada Ordenada
1. **UI**: Pressione `Ctrl+C` no terminal da UI
2. **API**: Pressione `Ctrl+C` no terminal da API
3. Aguarde finalização completa

### Verificação
- Portas 5001 e 5002 liberadas
- Processos encerrados
- Logs salvos automaticamente

---

## 📚 Recursos Adicionais

### Endpoints da API
- **Health**: http://localhost:5002/health
- **Telemetria**: http://localhost:5002/api/telemetry
- **Dashboard**: http://localhost:5002/api/dashboard
- **Swagger**: http://localhost:5002/swagger

### Arquivos de Configuração
- **API**: `api/appsettings.json`
- **UI**: `ui/Program.cs`
- **Estilos**: `ui/wwwroot/css/scada.css`

### Logs do Sistema
- Console da API: Telemetria em tempo real
- Console da UI: Eventos de interface
- Logs automáticos: Armazenados pelo .NET

---

## 🎯 Conclusão

O SCADA do Forno Industrial oferece:
- ✅ **Monitoramento em tempo real**
- ✅ **Interface moderna e responsiva**
- ✅ **Controles intuitivos**
- ✅ **Sistema de segurança robusto**
- ✅ **Alarmes automáticos**

O sistema está pronto para uso em ambiente industrial, fornecendo controle total sobre o processo de aquecimento com segurança e precisão.

---

*📅 Última atualização: Setembro 2025*  
*🏷️ Versão: MOMENTO 3 - SCADA Dashboard*  
*👨‍💻 Sistema desenvolvido para Laboratório Avançado*
