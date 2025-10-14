# Tutorial Completo do Wokwi (VS Code, PlatformIO, Chips API, IoT e CI/CD)

> Guia prático e avançado para quem deseja usar o **Wokwi** de forma profissional/acadêmica, integrando ao **VS Code**, **PlatformIO**, **custom chips**, conectividade IoT, depuração e integração contínua.

---

## 1. Introdução

O **Wokwi** é um simulador de sistemas embarcados que funciona tanto na nuvem quanto **localmente** via VS Code. Ele é ideal para prototipagem rápida, ensino e P&D, suportando microcontroladores como **Arduino (AVR), ESP32, RISC-V, RP2040**, entre outros.

**Principais recursos:**
- Simulação rápida e visual
- Integração com **PlatformIO** para build local
- Suporte a **depuração GDB**
- Criação de **chips personalizados** via **Chips API**
- Simulação **offline** (sem depender da nuvem)
- **Wokwi CI** para testes automáticos de firmware
- Recursos avançados de **IoT (Wi-Fi, MQTT, HTTP)** com ESP32

---

## 2. Instalação e Configuração

### 2.1 Pré-requisitos
- VS Code (última versão)
- Extensão **PlatformIO IDE**
- Extensão **Wokwi Embedded Simulator**
- Toolchains específicas (ex.: ESP32 `xtensa-esp32-elf-gdb`)

### 2.2 Instalação das Extensões
1. No VSCode, vá em `Extensões` → pesquise **PlatformIO IDE** → instale.
2. Pesquise **Wokwi Simulator** → instale.
3. Ative licença local:  
   - `Ctrl+Shift+P` → `Wokwi: Request a new License`
   - Ou insira chave manual via `Wokwi: Manually Enter License Key`

### 2.3 Criando um Projeto PlatformIO
```ini
; platformio.ini
[env:esp32dev]
platform = espressif32
framework = arduino
board = esp32dev
monitor_speed = 115200
```

### 2.4 Arquivo `wokwi.toml`
```toml
[wokwi]
version = 1
firmware = ".pio/build/esp32dev/firmware.bin"
elf      = ".pio/build/esp32dev/firmware.elf"
gdbServerPort = 3333
```

### 2.5 Criando o `diagram.json`
```json
{
  "version": 1,
  "parts": [
    { "id": "esp", "type": "board-esp32-devkitc" },
    { "id": "led1", "type": "wokwi-led", "attrs": { "color": "green" } }
  ],
  "connections": [
    [ "esp:TX", "$serialMonitor:RX", "" ],
    [ "esp:RX", "$serialMonitor:TX", "" ],
    [ "esp:2", "led1:A", "" ],
    [ "led1:C", "esp:GND", "" ]
  ]
}
```

---

## 3. Simulação Local e Offline

- Compile o firmware com PlatformIO.
- Inicie o simulador: `Ctrl+Shift+P` → **Wokwi: Start Simulator**
- O simulador carrega o `.bin` e `.elf` definidos no `wokwi.toml`.
- Para trabalhar 100% offline, basta ter a licença local ativada.

---

## 4. Depuração com GDB

1. No `wokwi.toml`, inclua `gdbServerPort = 3333`.
2. Em `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Wokwi GDB",
      "type": "cppdbg",
      "request": "launch",
      "program": "${workspaceFolder}/.pio/build/esp32dev/firmware.elf",
      "cwd": "${workspaceFolder}",
      "MIMode": "gdb",
      "miDebuggerPath": "~/.platformio/packages/toolchain-xtensa-esp32/bin/xtensa-esp32-elf-gdb",
      "miDebuggerServerAddress": "localhost:3333"
    }
  ]
}
```

- Compile, inicie o simulador e depois o debugger (F5).

---

## 5. Comunicação Serial Avançada

- **Monitor Serial:** `pio device monitor` ou painel do VSCode.
- **Múltiplas Seriais no ESP32:** use `HardwareSerial`.
```cpp
#include <HardwareSerial.h>
HardwareSerial Serial2(2);
void setup() {
  Serial.begin(115200);
  Serial2.begin(9600, SERIAL_8N1, 16, 17);
}
void loop() {
  Serial.println("Mensagem Serial0");
  Serial2.println("Mensagem Serial2");
  delay(1000);
}
```

- No `diagram.json`:
```json
[
  [ "esp:TX", "$serialMonitor:RX", "" ],
  [ "esp:RX", "$serialMonitor:TX", "" ],
  [ "esp:16", "$serial2Monitor:RX", "" ],
  [ "$serial2Monitor:TX", "esp:17", "" ]
]
```

---

## 6. Conectividade IoT com ESP32

### 6.1 Wi-Fi
```cpp
#include <WiFi.h>
const char* ssid = "Wokwi-GUEST";
const char* password = "";
void setup() {
  Serial.begin(115200);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) delay(500);
  Serial.println("WiFi Conectado!");
}
void loop() {}
```

### 6.2 MQTT (PubSubClient)
```cpp
#include <WiFi.h>
#include <PubSubClient.h>
WiFiClient espClient;
PubSubClient client(espClient);
void setup() {
  WiFi.begin("Wokwi-GUEST", "");
  client.setServer("broker.hivemq.com", 1883);
}
void loop() {
  if (!client.connected()) client.connect("esp32Client");
  client.publish("teste/wokwi", "Mensagem!");
  delay(2000);
}
```

### 6.3 HTTP
Use `HTTPClient` para GET/POST em APIs REST locais ou em nuvem.

---

## 7. Chips API — Criando Chips Personalizados

### 7.1 Definição JSON
`custom-chip/mychip.chip.json`:
```json
{
  "name": "mychip",
  "pins": [
    {"name":"VCC","type":"power"},
    {"name":"GND","type":"power"},
    {"name":"SIG","type":"digital"}
  ]
}
```

### 7.2 Código C
`custom-chip/mychip.c`:
```c
#include "wokwi-api.h"
DECLARE_PIN(SIG);

void *EXPORT(chip_init)(void) {
  pinMode(SIG, INPUT);
  return NULL;
}

void EXPORT(chip_tick)(void *chip) {
  if(digitalRead(SIG)){
    // lógica custom
  }
}
```

### 7.3 Build para WASM
Use **Emscripten** ou fluxo oficial para gerar `mychip.wasm`:
```bash
emcc mychip.c -Os -s STANDALONE_WASM -o mychip.wasm
```

### 7.4 Uso no diagrama
```json
{
  "id": "mychip1",
  "type": "chip-mychip",
  "attrs": {}
}
```

---

## 8. Wokwi CI com GitHub Actions

`.github/workflows/wokwi-ci.yml`:
```yaml
name: Wokwi CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build
        run: pio run
      - name: Test with Wokwi
        uses: wokwi/wokwi-ci-action@v1
        with:
          token: ${{ secrets.WOKWI_CLI_TOKEN }}
          path: ./
          expect_text: "WiFi Conectado!"
```

---

## 9. Boas Práticas e Troubleshooting

- **Paths corretos:** sempre confira `firmware` e `elf` no `wokwi.toml`.
- **Editor de diagrama:** chips customizados precisam de `.wasm` acessível.
- **Depuração:** iniciar simulador antes do debugger.
- **CI:** use `expect_text` para validar saída serial.

---

## 10. Recursos Úteis

- [Documentação oficial](https://docs.wokwi.com)
- [Extensão VS Code](https://marketplace.visualstudio.com/items?itemName=Wokwi.wokwi-vscode)
- [Chips API](https://docs.wokwi.com/chips-api)
- [Wokwi CI Action](https://github.com/wokwi/wokwi-ci-action)
- [Tutorial ESP32 + VSCode](https://www.espboards.dev/blog/use-wokwi-in-vscode-esp32)
