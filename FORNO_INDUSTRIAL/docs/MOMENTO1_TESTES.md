# MOMENTO 1 - TESTES PRÁTICOS
## Forno Industrial Inteligente - Arduino UNO

### 🎯 OBJETIVOS DO MOMENTO 1
- [x] Código limpo e não bloqueante (usar millis())
- [x] Modularização em funções (processAuto, processManual, publishTelemetry)
- [x] Interrupções para E-STOP com medição de latência
- [x] Métricas de tempo real (DT_ESTOP_US < 50ms, LOOP_US)

### 🔧 COMPILAÇÃO E EXECUÇÃO

#### 1. Compilar o código "BOM" (sem delay):
```bash
pio run -e uno
```

#### 2. Compilar o código "RUIM" (com delay):
```bash
pio run -e uno_bad
```

### 📊 TESTES DE FUNCIONALIDADE

#### 1. Teste básico - Telemetria automática (10Hz)
- Abra o Monitor Serial (115200 baud)
- Execute o comando: `GET;`
- Observe a telemetria JSON sendo enviada automaticamente

#### 2. Teste de controle automático
```
START;          # Inicia o processo
SET_SP=200;     # Define setpoint para 200°C
MODE=AUTO;      # Modo automático
GET;           # Visualiza estado
```

#### 3. Teste de controle manual
```
MODE=MAN;      # Modo manual
MAN=150,100;   # PWM: heater=150, fan=100
GET;           # Visualiza estado
```

#### 4. Teste crítico - E-STOP (Latência < 50ms)
- Pressione o botão vermelho E-STOP no simulador
- Observe na telemetria: `"dtEstopUs":XXXX`
- **META: dtEstopUs < 50000 (50ms)**
- Reset: `RST_ESTOP;`

### 📈 MÉTRICAS ESPERADAS

#### Código BOM (uno):
- **E-STOP Latency:** < 50ms (< 50000 μs)
- **Loop Time:** < 1000 μs (estável)
- **Telemetria:** 10Hz (a cada ~100ms)

#### Código RUIM (uno_bad):
- **E-STOP Latency:** ~50ms + delay (> 50000 μs)
- **Loop Time:** ~50000 μs (por causa do delay)
- **Telemetria:** Irregular

### 🔍 ANÁLISE COMPARATIVA

| Métrica | Código BOM | Código RUIM | Observação |
|---------|------------|-------------|------------|
| E-STOP Response | < 50ms | > 50ms | Delay bloqueia ISR |
| Loop Stability | Estável | Irregular | delay() causa jitter |
| Telemetria | 10Hz | < 10Hz | Bloqueios afetam timing |

### 🚨 COMANDOS COMPLETOS DE TESTE

```bash
# Sistema básico
GET;
START;
GET;

# Teste de limites
SET_LIMS=100,250,300;
SET_SP=280;
GET;

# Teste manual
MODE=MAN;
MAN=255,0;    # Aquecedor máximo
GET;
MAN=0,255;    # Ventilador máximo
GET;

# Parada de emergência
# (Pressionar botão E-STOP)
GET;          # Verificar dtEstopUs
RST_ESTOP;    # Reset do E-STOP
GET;

# Parada normal
STOP;
GET;
```

### ✅ CRITÉRIOS DE APROVAÇÃO

- [ ] **Compilação:** Ambos os códigos compilam sem erro
- [ ] **E-STOP:** Latência < 50ms no código BOM
- [ ] **Diferença:** Código RUIM mostra latência maior
- [ ] **Telemetria:** 10Hz estável no código BOM
- [ ] **Modularização:** Funções processAuto, processManual, publishTelemetry
- [ ] **Sem travamento:** Código responde a comandos sem delay

### 🎓 DEMONSTRAÇÃO DIDÁTICA

1. **Execute código BOM** - observe métricas normais
2. **Execute código RUIM** - observe degradação
3. **Compare resultados** - evidencie impacto do delay()
4. **Teste E-STOP** - demonstre diferença de latência

---
**MOMENTO 1 CONCLUÍDO:** Sistema de tempo real funcional com métricas demonstráveis! ✨
