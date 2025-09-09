#include <Arduino.h>

void setup() {
  Serial.begin(115200);
  pinMode(5, OUTPUT);  // LED vermelho (heater)
  pinMode(6, OUTPUT);  // LED azul (fan)
  pinMode(2, INPUT_PULLUP);  // Botão E-STOP
  
  Serial.println("=== TESTE SIMPLES INICIADO ===");
  Serial.println("LED Vermelho: Pino 5");
  Serial.println("LED Azul: Pino 6");
  Serial.println("Botao E-STOP: Pino 2");
  Serial.println("===============================");
}

void loop() {
  static unsigned long lastTime = 0;
  unsigned long now = millis();
  
  // Pisca LEDs alternadamente a cada segundo
  if (now - lastTime > 1000) {
    static bool state = false;
    
    if (state) {
      digitalWrite(5, HIGH);  // LED vermelho ON
      digitalWrite(6, LOW);   // LED azul OFF
      Serial.println("HEATER ON, FAN OFF");
    } else {
      digitalWrite(5, LOW);   // LED vermelho OFF
      digitalWrite(6, HIGH);  // LED azul ON
      Serial.println("HEATER OFF, FAN ON");
    }
    
    state = !state;
    lastTime = now;
  }
  
  // Verifica botão E-STOP
  if (digitalRead(2) == LOW) {
    digitalWrite(5, LOW);
    digitalWrite(6, LOW);
    Serial.println("*** E-STOP ATIVADO ***");
    delay(100);
  }
  
  delay(10);
}
