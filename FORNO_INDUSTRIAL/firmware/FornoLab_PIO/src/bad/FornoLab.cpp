#include <Arduino.h>
#ifndef IRAM_ATTR
#define IRAM_ATTR
#endif

const uint8_t PIN_HEATER = 5, PIN_FAN = 6, PIN_ESTOP = 2;

volatile bool v_eStop = false;
volatile unsigned long v_tEstopUs = 0;

bool running = false;
float T=180, SETP=180, LMIN=120, LMAX=220, LCRIT=240;   // <-- era SP
int pwmHeater=0, pwmFan=0;
unsigned long dtEstopUs=0, loopUs=0;
char lastState[10]="IDLE", lastAlarm[10]="NONE";

void IRAM_ATTR isrEstop() { v_eStop = true; v_tEstopUs = micros(); }

static inline void applyOutputs(int h,int f){
  pwmHeater=constrain(h,0,255); pwmFan=constrain(f,0,255);
  analogWrite(PIN_HEATER,pwmHeater); analogWrite(PIN_FAN,pwmFan);
}
static inline void allOff(){ applyOutputs(0,0); }

void sendTelemetry(){
  Serial.print("{\"t\":");Serial.print(T,1);
  Serial.print(",\"sp\":");Serial.print(SETP,1);
  Serial.print(",\"limMin\":");Serial.print(LMIN,1);
  Serial.print(",\"limMax\":");Serial.print(LMAX,1);
  Serial.print(",\"limCrit\":");Serial.print(LCRIT,1);
  Serial.print(",\"mode\":\"BAD\"");
  Serial.print(",\"state\":\"");Serial.print(lastState);Serial.print("\"");
  Serial.print(",\"heater\":");Serial.print(pwmHeater);
  Serial.print(",\"fan\":");Serial.print(pwmFan);
  Serial.print(",\"alarm\":\"");Serial.print(lastAlarm);Serial.print("\"");
  Serial.print(",\"eStop\":");Serial.print(v_eStop?"true":"false");
  Serial.print(",\"dtEstopUs\":");Serial.print(dtEstopUs);
  Serial.print(",\"loopUs\":");Serial.print(loopUs);
  Serial.print(",\"tsMs\":");Serial.print(millis());
  Serial.println("}");
}

// parser tosco
char ibuf[96]; uint8_t ilen=0;
void resetInput(){ilen=0;ibuf[0]=0;}
bool sw(const char*s){return strncmp(ibuf,s,strlen(s))==0;}

void handleCommand(){
  if (ilen>0 && ibuf[ilen-1]==';') ibuf[ilen-1]=0;
  if(sw("START")){running=true;Serial.println("ACK;");}
  else if(sw("STOP")){running=false;allOff();Serial.println("ACK;");}
  else if(sw("RST_ESTOP")){v_eStop=false;dtEstopUs=0;Serial.println("ACK;");}
  else if(sw("SET_SP=")){SETP=atof(ibuf+7);Serial.println("ACK;");}
  else if(sw("SET_LIMS=")){float a,b,c;if(sscanf(ibuf+9,"%f,%f,%f",&a,&b,&c)==3){LMIN=a;LMAX=b;LCRIT=c;Serial.println("ACK;");}else Serial.println("ERR;");}
  else if(sw("MAN=")){int h,f;if(sscanf(ibuf+4,"%d,%d",&h,&f)==2){applyOutputs(h,f);Serial.println("ACK;");}else Serial.println("ERR;");}
  else if(sw("GET")){sendTelemetry();}
  else Serial.println("ERR;");
}

static inline void updatePlant(float dt){ const float amb=25.0f; float heat=(pwmHeater/255.0f)*6.0f, cool=(pwmFan/255.0f)*6.0f; T += (heat-cool)*dt; T += (amb-T)*0.03f*dt; }

void setup(){
  pinMode(PIN_HEATER,OUTPUT); pinMode(PIN_FAN,OUTPUT); pinMode(PIN_ESTOP,INPUT_PULLUP);
  allOff(); Serial.begin(115200);
  attachInterrupt(digitalPinToInterrupt(PIN_ESTOP),isrEstop,FALLING);
  Serial.println("ACK;");
}

void loop(){
  unsigned long t0=micros();

  // 👎 loop bloqueante de propósito (didático)
  delay(50);

  while(Serial.available()){
    char c=Serial.read(); if(ilen<sizeof(ibuf)-1) ibuf[ilen++]=c;
    if(c==';'){ibuf[ilen]=0; handleCommand(); resetInput(); }
  }

  if(v_eStop){
    allOff();                         // só desliga depois do delay(50) -> intencionalmente ruim
    dtEstopUs = micros() - v_tEstopUs;
    strcpy(lastState,"E_STOP");
  }

  static unsigned long lastTick=0, now=millis();
  now = millis();
  if(now-lastTick>=100){ updatePlant((now-lastTick)/1000.0f); lastTick=now; }

  if(T>=LCRIT) strcpy(lastAlarm,"CRIT");
  else if(T>LMAX) strcpy(lastAlarm,"HIGH");
  else if(T<LMIN) strcpy(lastAlarm,"LOW");
  else strcpy(lastAlarm,"NONE");

  loopUs=micros()-t0;
}
