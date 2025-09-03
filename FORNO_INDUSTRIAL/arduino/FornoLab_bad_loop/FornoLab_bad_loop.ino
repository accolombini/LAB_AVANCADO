// ==============================================
// FornoLab.ino — Controle de Forno c/ E-STOP, PWM e Telemetria
// Placa: Arduino UNO (ATmega328P) — Baud 115200
// Protocolo: linhas 'chave=valor;' separadas por \n
// ==============================================

#include <Arduino.h>

// ------------ Pinos (UNO) ------------
const uint8_t PIN_TEMP    = A0; // LM35 / TMP36
const uint8_t PIN_HEATER  = 5;  // PWM Heater (SSR/Driver)
const uint8_t PIN_FAN     = 6;  // PWM Fan
const uint8_t PIN_BUZZER  = 9;  // Buzzer simples
const uint8_t PIN_LED_OK  = 10; // LED status OK
const uint8_t PIN_LED_ALR = 11; // LED alarme
const uint8_t PIN_ESTOP   = 2;  // INT0, botão emergência (GND ativa)

// ------------ Estados & Modos ------------
enum Mode  { AUTO, MANUAL };
enum State { IDLE, HEATING, COOLING, CRITICAL_SHUTDOWN };

// ------------ Parâmetros ------------
volatile bool g_estop = false;          // setado em ISR
float g_setpoint = 180.0f;              // °C
float g_lim_min = 120.0f, g_lim_max = 220.0f, g_lim_crit = 240.0f;
Mode  g_mode  = AUTO;
State g_state = IDLE;

// Controle
const float HYST = 2.0f;                 // histerese °C
uint8_t g_pwm_heater = 0;                // 0..255
uint8_t g_pwm_fan    = 0;                // 0..255

// Amostragem e métricas
unsigned long g_lastSampleMs = 0;
const unsigned long SAMPLE_MS = 100;     // 10 Hz
unsigned long g_lastLoopUs = 0;          // tempo do último ciclo (µs)
volatile unsigned long g_estopTriggerUs = 0; // carimbo da ISR (µs)
unsigned long g_dtEstopUs = 0;           // latência até outputs zerarem (µs)

// Buffer de recepção serial
char rxbuf[128];
uint8_t rxi = 0;

// ------------ ISR E-STOP ------------
void onEstop() {
  g_estop = true;
  g_estopTriggerUs = micros();
}

// ------------ Utilitários ------------
float readTemperatureC() {
  int adc = analogRead(PIN_TEMP);
  // LM35: 10mV/°C, V = adc*(5/1023), T = V/0.01
  return (adc * 500.0f) / 1023.0f;
}

void applyOutputs(uint8_t heater, uint8_t fan) {
  analogWrite(PIN_HEATER, heater);
  analogWrite(PIN_FAN, fan);
}

void outputsSafeOff() {
  g_pwm_heater = 0;
  g_pwm_fan = 255; // arrefece no pânico
  applyOutputs(g_pwm_heater, g_pwm_fan);
}

void setAlarms(bool on) {
  digitalWrite(PIN_LED_ALR, on ? HIGH : LOW);
  if (on) tone(PIN_BUZZER, 2000); else noTone(PIN_BUZZER);
}

void publishTelemetry(float tempC) {
  Serial.print("T="); Serial.print(tempC, 1);
  Serial.print(";SP="); Serial.print(g_setpoint, 1);
  Serial.print(";LIM_MIN="); Serial.print(g_lim_min, 1);
  Serial.print(";LIM_MAX="); Serial.print(g_lim_max, 1);
  Serial.print(";LIM_CRIT="); Serial.print(g_lim_crit, 1);
  Serial.print(";MODE="); Serial.print(g_mode == AUTO ? "AUTO" : "MAN");
  Serial.print(";STATE=");
  switch (g_state) {
    case IDLE: Serial.print("IDLE"); break;
    case HEATING: Serial.print("HEATING"); break;
    case COOLING: Serial.print("COOLING"); break;
    case CRITICAL_SHUTDOWN: Serial.print("CRITICAL"); break;
  }
  Serial.print(";HEATER="); Serial.print(g_pwm_heater);
  Serial.print(";FAN="); Serial.print(g_pwm_fan);
  Serial.print(";ALARM=");
  bool alarm = (g_state == CRITICAL_SHUTDOWN) || (tempC > g_lim_max + 5.0f) || (tempC < g_lim_min - 5.0f);
  Serial.print(alarm ? "ON" : "NONE");
  Serial.print(";ESTOP="); Serial.print(g_estop ? 1 : 0);
  Serial.print(";DT_ESTOP_US="); Serial.print(g_dtEstopUs);
  Serial.print(";LOOP_US="); Serial.print(g_lastLoopUs);
  Serial.println(";");
}

void processAuto(float tempC) {
  if (tempC >= g_lim_crit) {
    g_state = CRITICAL_SHUTDOWN;
    outputsSafeOff();
    setAlarms(true);
    return;
  }

  setAlarms(false);

  if (tempC < g_setpoint - HYST) {
    g_state = HEATING;
    g_pwm_heater = 200;
    g_pwm_fan = 0;
  } else if (tempC > g_setpoint + HYST) {
    g_state = COOLING;
    g_pwm_heater = 0;
    g_pwm_fan = 180;
  } else {
    g_state = IDLE;
    g_pwm_heater = 0;
    g_pwm_fan = 0;
  }

  applyOutputs(g_pwm_heater, g_pwm_fan);
}

void processManual() {
  setAlarms(false);
  g_state = IDLE;
  applyOutputs(g_pwm_heater, g_pwm_fan);
}

void handleCommand(const char* line) {
  String s(line);
  s.trim(); s.toUpperCase();

  if (s == "GET;") { Serial.println("ACK;"); return; }
  if (s == "START;") { g_estop = false; Serial.println("ACK;"); return; }
  if (s == "STOP;")  { outputsSafeOff(); Serial.println("ACK;"); return; }
  if (s == "RST_ESTOP;") { g_estop = false; Serial.println("ACK;"); return; }

  if (s.startsWith("MODE=")) {
    if (s.indexOf("AUTO;") > 0) g_mode = AUTO; else g_mode = MANUAL;
    Serial.println("ACK;");
    return;
  }

  if (s.startsWith("SET_SP=")) {
    int eq = s.indexOf('='); int sc = s.indexOf(';');
    if (eq>0 && sc>eq) {
      g_setpoint = s.substring(eq+1, sc).toFloat();
      Serial.println("ACK;");
      return;
    }
  }

  if (s.startsWith("SET_LIMS=")) {
    int eq = s.indexOf('='); int sc = s.indexOf(';');
    if (eq>0 && sc>eq) {
      String vals = s.substring(eq+1, sc); // MIN,MAX,CRIT
      int c1 = vals.indexOf(',');
      int c2 = vals.indexOf(',', c1+1);
      if (c1>0 && c2>c1) {
        g_lim_min  = vals.substring(0, c1).toFloat();
        g_lim_max  = vals.substring(c1+1, c2).toFloat();
        g_lim_crit = vals.substring(c2+1).toFloat();
        Serial.println("ACK;");
        return;
      }
    }
  }

  if (s.startsWith("MAN=")) {
    int eq = s.indexOf('='); int sc = s.indexOf(';');
    if (eq>0 && sc>eq) {
      String vals = s.substring(eq+1, sc);
      int c = vals.indexOf(',');
      if (c>0) {
        g_pwm_heater = constrain(vals.substring(0, c).toInt(), 0, 255);
        g_pwm_fan    = constrain(vals.substring(c+1).toInt(), 0, 255);
        Serial.println("ACK;");
        return;
      }
    }
  }

  Serial.println("ERR=CMD;");
}

void setup() {
  pinMode(PIN_TEMP, INPUT);
  pinMode(PIN_HEATER, OUTPUT);
  pinMode(PIN_FAN, OUTPUT);
  pinMode(PIN_BUZZER, OUTPUT);
  pinMode(PIN_LED_OK, OUTPUT);
  pinMode(PIN_LED_ALR, OUTPUT);
  pinMode(PIN_ESTOP, INPUT_PULLUP); // botão para GND

  Serial.begin(115200);
  attachInterrupt(digitalPinToInterrupt(PIN_ESTOP), onEstop, FALLING);

  outputsSafeOff();
  digitalWrite(PIN_LED_OK, HIGH);
}

void loop() {
  const unsigned long EXTRA_DELAY_MS = 50; // DEMO: atraso intencional para latência
  unsigned long t0 = micros();

  // Recepção
  while (Serial.available()) {
    char c = (char)Serial.read();
    if (c == '\n' || c == '\r') {
      rxbuf[rxi] = '\0';
      if (rxi > 0) handleCommand(rxbuf);
      rxi = 0;
    } else if (rxi < sizeof(rxbuf)-1) {
      rxbuf[rxi++] = c;
    }
  }

  // E-STOP tratamento imediato
  if (g_estop) {
    outputsSafeOff();
    setAlarms(true);
    g_state = CRITICAL_SHUTDOWN;
    if (g_dtEstopUs == 0 && g_estopTriggerUs != 0) {
      g_dtEstopUs = micros() - g_estopTriggerUs;
    }
  }

  // Amostragem e controle (não bloqueante)
  unsigned long now = millis();
  if (!g_estop && (now - g_lastSampleMs) >= SAMPLE_MS) {
    g_lastSampleMs = now;

    float tempC = readTemperatureC();
    if (g_mode == AUTO) processAuto(tempC);
    else                processManual();

    publishTelemetry(tempC);
  }

  g_lastLoopUs = micros() - t0;
  delay(EXTRA_DELAY_MS);
}
