#include <Arduino.h>

// ========== CONFIGURAÇÕES DO FORNO INDUSTRIAL ==========
const float TEMP_AMBIENTE = 25.0;        // Temperatura ambiente (°C)
const float TEMP_MINIMA = 1000.0;        // Temperatura mínima de operação (°C)
const float TEMP_MAXIMA = 1800.0;        // Temperatura máxima de operação (°C)
const float TEMP_SETPOINT = 1500.0;      // Temperatura de regime (°C)
const float TEMP_ALARME = 1600.0;        // Temperatura de alarme (°C)
const float TEMP_CRITICA = 1750.0;       // Temperatura crítica de interrupção (°C)

// ========== PINOS DE CONTROLE ==========
const int PIN_MACARICO = 5;              // Pino PWM do maçarico
const int PIN_VENTILADOR = 6;            // Pino PWM do ventilador
const int PIN_ALARME = 3;                // Pino do LED de alarme

// ========== PARÂMETROS DE SIMULAÇÃO TÉRMICA ==========
const float INCREMENTO_TEMP = 5.0;       // °C por ciclo (didático)
const float DECREMENTO_TEMP = 8.0;       // °C por ciclo (ventilador)
const float PERDA_AMBIENTE = 2.0;        // °C por ciclo (perdas naturais)

// ========== VARIÁVEIS GLOBAIS ==========
float temperatura_atual = 1400.0;        // Inicia abaixo do setpoint para forçar aquecimento
bool sistema_ativo = true;
bool alarme_ativo = false;
bool interrupcao_critica = false;
bool macarico_ligado = false;
bool ventilador_ligado = false;

// ========== CONFIGURAÇÃO INICIAL ==========
void setup() {
  Serial.begin(115200);
  
  // Configurar pinos como saída
  pinMode(PIN_MACARICO, OUTPUT);
  pinMode(PIN_VENTILADOR, OUTPUT);
  pinMode(PIN_ALARME, OUTPUT);
  
  // Estado inicial - todos desligados
  digitalWrite(PIN_MACARICO, LOW);
  digitalWrite(PIN_VENTILADOR, LOW);
  digitalWrite(PIN_ALARME, LOW);
  
  Serial.println("=== FORNO INDUSTRIAL - MOMENTO 1 ===");
  Serial.println("Temperatura: 1000-1800C");
  Serial.println("Regime: 1500C");
  Serial.println("Alarme: 1600C");
  Serial.println("Interrupcao: 1750C");
  Serial.println("Sistema iniciado!");
  Serial.println();
}

// ========== VERIFICAÇÃO DE TEMPERATURA CRÍTICA ==========
void verificar_temperatura_critica() {
  // INTERRUPÇÃO CRÍTICA aos 1750°C (PRIORIDADE MÁXIMA)
  if (temperatura_atual >= TEMP_CRITICA && !interrupcao_critica) {
    interrupcao_critica = true;
    sistema_ativo = false;
    
    Serial.println();
    Serial.println("*** INTERRUPCAO CRITICA! TEMP >= 1750C ***");
    Serial.println("*** VENTILADORES EM FORCA TOTAL ***");
    Serial.println();
  }
  
  // RESETAR INTERRUPÇÃO quando temperatura voltar ao setpoint
  if (interrupcao_critica && temperatura_atual <= TEMP_SETPOINT) {
    interrupcao_critica = false;
    sistema_ativo = true;
    alarme_ativo = false; // Reset do alarme também
    
    Serial.println();
    Serial.println("*** SISTEMA REATIVADO - TEMP <= 1500C ***");
    Serial.println("*** REINICIANDO CICLO DE AQUECIMENTO ***");
    Serial.println();
  }
  
  // ALARME aos 1600°C (só se não estiver em interrupção)
  if (!interrupcao_critica && temperatura_atual >= TEMP_ALARME && !alarme_ativo) {
    alarme_ativo = true;
    digitalWrite(PIN_ALARME, HIGH);
    Serial.println("*** ALARME! TEMPERATURA CRITICA >= 1600C ***");
  }
  
  // Desligar alarme se temperatura baixou (e não está em interrupção)
  if (!interrupcao_critica && temperatura_atual < TEMP_ALARME && alarme_ativo) {
    alarme_ativo = false;
    digitalWrite(PIN_ALARME, LOW);
  }
  
  // Manter alarme ligado durante interrupção crítica
  if (interrupcao_critica) {
    digitalWrite(PIN_ALARME, HIGH);
  }
}

// ========== CONTROLE DE TEMPERATURA ==========
void controlar_temperatura() {
  // PRIORIDADE 1: Se sistema em interrupção crítica, só ventilador força total
  if (interrupcao_critica) {
    macarico_ligado = false;
    ventilador_ligado = true;
    digitalWrite(PIN_MACARICO, LOW);
    digitalWrite(PIN_VENTILADOR, HIGH);
    return; // Sai da função - nada mais importa
  }
  
  // PRIORIDADE 2: Sistema normal - sempre aquece até atingir 1750°C
  if (sistema_ativo) {
    // CONTINUA AQUECENDO até atingir temperatura crítica
    macarico_ligado = true;
    ventilador_ligado = false;
    digitalWrite(PIN_MACARICO, HIGH);
    digitalWrite(PIN_VENTILADOR, LOW);
  } else {
    // Sistema inativo - desligar tudo
    macarico_ligado = false;
    ventilador_ligado = false;
    digitalWrite(PIN_MACARICO, LOW);
    digitalWrite(PIN_VENTILADOR, LOW);
  }
}

// ========== SIMULAÇÃO TÉRMICA ==========
void simular_temperatura() {
  // AQUECIMENTO: +5°C quando maçarico ligado
  if (macarico_ligado) {
    temperatura_atual += INCREMENTO_TEMP; // +5°C
    Serial.print(">>> AQUECENDO +5C ");
  }
  
  // RESFRIAMENTO: -8°C quando ventilador ligado
  if (ventilador_ligado) {
    temperatura_atual -= DECREMENTO_TEMP; // -8°C
    Serial.print(">>> RESFRIANDO -8C ");
  }
  
  // PERDAS NATURAIS: sempre perde 1°C (só quando não está aquecendo)
  if (!macarico_ligado && temperatura_atual > TEMP_AMBIENTE) {
    temperatura_atual -= 1.0; // Perda natural sempre 1°C
    if (!ventilador_ligado) {
      Serial.print(">>> PERDA NATURAL -1C ");
    }
  }
  
  // LIMITES FÍSICOS
  if (temperatura_atual < TEMP_AMBIENTE) {
    temperatura_atual = TEMP_AMBIENTE;
  }
  if (temperatura_atual > TEMP_MAXIMA) {
    temperatura_atual = TEMP_MAXIMA;
  }
}

// ========== EXIBIR STATUS ==========
void exibir_status() {
  Serial.print("TEMP: ");
  Serial.print(temperatura_atual, 1);
  Serial.print("C | SP: ");
  Serial.print(TEMP_SETPOINT, 0);
  Serial.print("C | ");
  
  if (interrupcao_critica) {
    Serial.print("*** INTERRUPCAO CRITICA ***");
  } else if (macarico_ligado) {
    Serial.print("AQUECENDO (+5C)");
  } else if (ventilador_ligado) {
    Serial.print("RESFRIANDO (-8C)");
  } else {
    Serial.print("MANTENDO (-1C)");
  }
  
  Serial.print(" | M:");
  Serial.print(macarico_ligado ? "ON" : "OFF");
  Serial.print(" V:");
  Serial.print(ventilador_ligado ? "ON" : "OFF");
  Serial.print(" A:");
  Serial.print(alarme_ativo ? "ON" : "OFF");
  
  if (temperatura_atual >= TEMP_ALARME && temperatura_atual < TEMP_CRITICA) {
    Serial.print(" *** ALARME ***");
  }
  
  Serial.println();
}

// ========== LOOP PRINCIPAL ==========
void loop() {
  // 1. Verificar temperatura crítica (PRIORIDADE MÁXIMA)
  verificar_temperatura_critica();
  
  // 2. Controlar temperatura (SEMPRE - independente do sistema_ativo)
  controlar_temperatura();
  
  // 3. Simular planta térmica
  simular_temperatura();
  
  // 4. Exibir status
  exibir_status();
  
  // 5. Aguardar próximo ciclo
  delay(1000); // 1 segundo por ciclo
}