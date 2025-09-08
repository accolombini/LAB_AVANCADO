#include <Arduino.h>
#ifndef IRAM_ATTR
#define IRAM_ATTR
#endif

// ===== Pinos (UNO) =====
const uint8_t PIN_HEATER = 5;   // PWM
const uint8_t PIN_FAN    = 6;   // PWM
const uint8_t PIN_ESTOP  = 2;   // INT0 (D2)

// ===== Estado de processo =====
enum Mode { MODE_AUTO, MODE_MAN };
volatile bool v_eStop = false;
volatile unsigned long v_tEstopUs = 0;

bool running = false;
Mode mode = MODE_AUTO;

float T = 180.0f;          // temperatura simulada (°C)
float SETP = 180.0f;       // <-- era SP (colidia com macro do AVR)
float LMIN = 120.0f, LMAX = 220.0f, LCRIT = 240.0f;

int pwmHeater = 0;         // 0..255
int pwmFan    = 0;         // 0..255
unsigned long dtEstopUs = 0;
unsigned long loopUs = 0;

char lastState[10] = "IDLE";
char lastAlarm[10] = "NONE";

// ===== Planta simulada (didática) =====
static inline void updatePlant(float dtSec) {
  const float amb = 25.0f;
  float heat = (pwmHeater / 255.0f) * 6.0f;
  float cool = (pwmFan    / 255.0f) * 6.0f;
  T += (heat - cool) * dtSec;
  T += (amb - T) * 0.03f * dtSec;
}

// ===== Saídas =====
static inline void applyOutputs(int h, int f) {
  pwmHeater = constrain(h, 0, 255);
  pwmFan    = constrain(f, 0, 255);
  analogWrite(PIN_HEATER, pwmHeater);
  analogWrite(PIN_FAN,    pwmFan);
}

static inline void allOff() { applyOutputs(0, 0); }

// ===== E-STOP ISR =====
void IRAM_ATTR isrEstop() {
  v_eStop = true;
  v_tEstopUs = micros();
}

// ===== Telemetria =====
void sendTelemetry() {
  Serial.print("{\"t\":");        Serial.print(T, 1);
  Serial.print(",\"sp\":");       Serial.print(SETP, 1);
  Serial.print(",\"limMin\":");   Serial.print(LMIN, 1);
  Serial.print(",\"limMax\":");   Serial.print(LMAX, 1);
  Serial.print(",\"limCrit\":");  Serial.print(LCRIT, 1);
  Serial.print(",\"mode\":\"");   Serial.print(mode == MODE_AUTO ? "AUTO" : "MAN"); Serial.print("\"");
  Serial.print(",\"state\":\"");  Serial.print(lastState); Serial.print("\"");
  Serial.print(",\"heater\":");   Serial.print(pwmHeater);
  Serial.print(",\"fan\":");      Serial.print(pwmFan);
  Serial.print(",\"alarm\":\"");  Serial.print(lastAlarm); Serial.print("\"");
  Serial.print(",\"eStop\":");    Serial.print(v_eStop ? "true" : "false");
  Serial.print(",\"dtEstopUs\":");Serial.print(dtEstopUs);
  Serial.print(",\"loopUs\":");   Serial.print(loopUs);
  Serial.print(",\"tsMs\":");     Serial.print(millis());
  Serial.println("}");
}

// ===== Parser simples "CMD;..." =====
char ibuf[96]; uint8_t ilen = 0;
void resetInput(){ ilen=0; ibuf[0]=0; }
bool startsWith(const char* s){ return strncmp(ibuf, s, strlen(s)) == 0; }

void handleCommand() {
  if (ilen>0 && ibuf[ilen-1]==';') ibuf[ilen-1]=0;
  if (startsWith("START")) { running = true; Serial.println("ACK;"); }
  else if (startsWith("STOP")) { running = false; allOff(); Serial.println("ACK;"); }
  else if (startsWith("RST_ESTOP")) { v_eStop=false; dtEstopUs=0; Serial.println("ACK;"); }
  else if (startsWith("MODE=AUTO")) { mode = MODE_AUTO; Serial.println("ACK;"); }
  else if (startsWith("MODE=MAN"))  { mode = MODE_MAN;  Serial.println("ACK;"); }
  else if (startsWith("SET_SP="))   { SETP = atof(ibuf+7); Serial.println("ACK;"); }
  else if (startsWith("SET_LIMS=")) { float a,b,c; if (sscanf(ibuf+9,"%f,%f,%f",&a,&b,&c)==3){ LMIN=a; LMAX=b; LCRIT=c; Serial.println("ACK;"); } else Serial.println("ERR;"); }
  else if (startsWith("MAN="))      { int h,f; if (sscanf(ibuf+4,"%d,%d",&h,&f)==2){ applyOutputs(h,f); Serial.println("ACK;"); } else Serial.println("ERR;"); }
  else if (startsWith("GET"))       { sendTelemetry(); }
  else                              { Serial.println("ERR;"); }
}

void setup() {
  pinMode(PIN_HEATER, OUTPUT);
  pinMode(PIN_FAN,    OUTPUT);
  pinMode(PIN_ESTOP,  INPUT_PULLUP);
  allOff();
  Serial.begin(115200);
  attachInterrupt(digitalPinToInterrupt(PIN_ESTOP), isrEstop, FALLING);
  Serial.println("ACK;");
}

void loop() {
  const unsigned long t0 = micros();

  // leitura de linha terminada por ';'
  while (Serial.available()) {
    char c = Serial.read();
    if (ilen < sizeof(ibuf)-1) ibuf[ilen++] = c;
    if (c == ';') { ibuf[ilen] = 0; handleCommand(); resetInput(); }
  }

  if (v_eStop) {
    allOff();
    dtEstopUs = micros() - v_tEstopUs;
    strcpy(lastState, "E_STOP");
  } else if (!running) {
    allOff();
    strcpy(lastState, "IDLE");
  } else {
    if (mode == MODE_AUTO) {
      // controle bang-bang bem simples
      if      (T < SETP - 0.5f) { applyOutputs(200, 0);  strcpy(lastState, "HEAT"); }
      else if (T > SETP + 0.5f) { applyOutputs(0, 200);  strcpy(lastState, "COOL"); }
      else                      { applyOutputs(0, 0);    strcpy(lastState, "IDLE"); }
    } else {
      strcpy(lastState, "MAN"); // mantém PWM ajustados
    }
  }

  // alarmes
  if      (T >= LCRIT) strcpy(lastAlarm, "CRIT");
  else if (T >  LMAX)  strcpy(lastAlarm, "HIGH");
  else if (T <  LMIN)  strcpy(lastAlarm, "LOW");
  else                 strcpy(lastAlarm, "NONE");

  // atualização da planta a cada ~100 ms
  static unsigned long lastTick = 0;
  unsigned long now = millis();
  if (now - lastTick >= 100) {
    updatePlant((now - lastTick) / 1000.0f);
    lastTick = now;
  }

  loopUs = micros() - t0;
}
