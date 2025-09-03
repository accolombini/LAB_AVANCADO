# Guia rápido — PlatformIO + Arduino Uno + API/UI .NET (Lab “Forno Industrial”)

> Objetivo: ter um fluxo **estável, repetível e sem prompts de login** para compilar/carregar o firmware no **Arduino Uno** via **PlatformIO**, e conectá-lo à nossa **API (.NET)** e **UI (Blazor)** do laboratório.

---

## 0) Pré-requisitos (macOS)

1. **VS Code (stable)**  
2. **Extensões VS Code (somente as necessárias)**
   - ✅ C/C++ — `ms-vscode.cpptools`
   - ✅ C# — `ms-dotnettools.csharp`
   - ✅ PlatformIO IDE — `platformio.platformio-ide`
   - ❌ **Desabilitar/Remover**: *Arduino (oficial)*, **C# Dev Kit**, **IntelliCode for C# Dev Kit**, Live Share (se não usar).
3. **.NET SDK 8.x** (padronize a mesma versão nas máquinas)  
   Verifique: `dotnet --info`  
   Opcional (na raiz do repo) fixe a versão com `global.json`:
   ```json
   { "sdk": { "version": "8.0.401", "rollForward": "latestFeature" } }
   ```
4. **PlatformIO Core**  
   - Normalmente a extensão baixa o Python interno (penv).  
   - *Se a rede bloquear downloads*, instale o **PIO Core do sistema** (não exige login):  
     ```bash
     python3 -m pip install --user platformio
     echo 'export PATH="$HOME/Library/Python/3.*/bin:$PATH"' >> ~/.zprofile
     exec zsh -l
     pio --version
     ```
     No VS Code (Settings JSON):
     ```json
     {
       "platformio-ide.useBuiltinPIOCore": false,
       "platformio-ide.customPATH": "/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin"
     }
     ```
5. **Higiene de ambiente (recomendado para aula)**  
   ```jsonc
   // settings.json (User)
   {
     "extensions.autoCheckUpdates": false,
     "extensions.autoUpdate": false
   }
   ```

---

## 1) Estrutura recomendada do repositório

```
FORNO_INDUSTRIAL/
├─ api/                      # .NET API (Minimal API)
├─ ui/                       # Blazor WebAssembly
├─ arduino/                  # sketches originais (referência)
│  ├─ FornoLab.ino
│  └─ FornoLab_bad_loop.ino
└─ firmware/
   └─ FornoLab_PIO/          # projeto PlatformIO
       ├─ platformio.ini
       └─ src/
          ├─ good/FornoLab.ino
          └─ bad/FornoLab.ino
```

> **.gitignore**: adicione `/.pio/` e artefatos temporários.

---

## 2) Momento “0 – decolagem” (5–10 min)

1. `dotnet --version` (verifique SDK).  
2. **API**:  
   ```bash
   cd api
   DOTNET_URLS=http://localhost:5199 dotnet run
   ```
3. **UI**:  
   ```bash
   cd ui
   DOTNET_URLS=http://localhost:5289 dotnet run
   ```
4. **Smoke test** (em outro terminal):  
   ```bash
   curl http://localhost:5199/health
   curl http://localhost:5199/api/telemetry        # pode dar 204 antes da 1ª amostra
   curl -X POST "http://localhost:5199/api/cmd" -H "Content-Type: application/json" -d "\"GET;\""
   curl http://localhost:5199/api/last
   ```
   Veja 1 linha de telemetria no simulador e a UI abrir em `http://localhost:5289`.

---

## 3) Criar o projeto PlatformIO (dois ambientes)

> **Caminhos com espaço** (ex.: *Mobile Documents*): sempre use **aspas**.

### Opção A — via GUI e mover
1. PIO Home → **New Project** → *Name*: `FornoLab_PIO`, *Board*: **Arduino Uno**, *Framework*: **Arduino** → Finish.  
2. Mova a pasta criada para `FORNO_INDUSTRIAL/firmware/FornoLab_PIO` e depois PIO Home → **Open Project**.

### Opção B — via CLI (direto no repo)
```bash
cd "<repo>/FORNO_INDUSTRIAL"
mkdir -p "firmware/FornoLab_PIO"
pio project init --board uno --project-dir "firmware/FornoLab_PIO"
```

### `platformio.ini` (substitua tudo)
```ini
[platformio]
default_envs = uno

[env:uno]                 ; versão "boa"
platform = atmelavr
board = uno
framework = arduino
monitor_speed = 115200
src_dir = src/good

[env:uno_bad]             ; versão com delay(50) p/ experimento de latência
platform = atmelavr
board = uno
framework = arduino
monitor_speed = 115200
src_dir = src/bad
```

> Apague o `src/main.cpp` gerado automaticamente.

### Coloque os sketches
```bash
mkdir -p firmware/FornoLab_PIO/src/good firmware/FornoLab_PIO/src/bad
cp arduino/FornoLab.ino                    firmware/FornoLab_PIO/src/good/FornoLab.ino
cp arduino/FornoLab_bad_loop.ino           firmware/FornoLab_PIO/src/bad/FornoLab.ino
```

---

## 4) Compilar / Upload / Monitor

### Pela interface (recomendado na aula)
- **PROJECT TASKS → FornoLab_PIO → uno → Build** → **Upload** → **Monitor** (115200).  
- Para o experimento, troque para **uno_bad** e **Upload**.

### Pela CLI (equivalente)
```bash
# Ambiente "bom"
pio run -e uno
pio run -e uno -t upload
pio device monitor -b 115200

# Ambiente "bad loop"
pio run -e uno_bad -t upload
```

**Porta serial (macOS):** geralmente `/dev/cu.usbmodem*` (clones CH340: `/dev/cu.wchusbserial*`).  
Liste portas: `pio device list`. Se precisar, force no upload:  
`pio run -e uno -t upload --upload-port /dev/cu.usbmodemXXXX`

> Erro `resource busy`: feche o **Serial Monitor** antes do *Upload*.

---

## 5) Integrar com a API/UI (hardware real)

1. Em `api/appsettings.json`:
```json
{ "UseSimulator": false, "SerialPort": "/dev/cu.usbmodemXXXX" }
```
2. Reinicie a **API** (5199) e a **UI** (5289).
3. Testes rápidos:
```bash
curl http://localhost:5199/health
curl -X POST "http://localhost:5199/api/cmd" -H "Content-Type: application/json" -d "\"GET;\""
```

**UI (Blazor)**: opere **Start / Stop / Auto / Manual**, ajuste **Setpoint** e **Limites**, veja **ACK/ERR** e a **tendência de 60s**.

---

## 6) Experimento de latência (aula)

1. Carregue a versão **uno_bad** (tem `delay(50)` dentro do `loop()` no firmware).  
2. Dispare **E-STOP** várias vezes e compare o tempo de resposta/telemetria com a versão **uno**.  
3. Discuta: *por que bloqueio no loop impacta controle/segurança?* → introdução a interrupções e design responsivo.

---

## 7) Troubleshooting rápido

- **Não compila `Arduino.h`**: você está no PlatformIO — não precisa do Arduino IDE/Core; apenas garanta `board = uno`.  
- **PenV travado/sem download**: use o **PIO Core via pip** (ver Pré-requisitos §4).  
- **Porta não aparece**: cabo/porta; `pio device list`. Em clones, driver CH340.  
- **Upload falha (timeout/avrdude)**: pressione **RESET** no UNO logo antes do upload; confirme a porta correta.  
- **“Resource busy”**: feche o monitor serial.  
- **API retorna 204 no `/api/telemetry`**: espere a 1ª amostra ou confirme conexão serial/porta em `appsettings.json`.
- **“resource busy”**: feche o *Serial Monitor* antes do upload.  
- **Timeout/avrdude**: confira a porta e pressione **RESET** no UNO logo antes do upload.  
- **Porta não aparece**: reconecte o cabo/USB; `pio device list`; clones CH340 precisam driver.  
- **Erro de build**: cheque `platformio.ini` e se não existe `src/main.cpp` extra.  
- **Caminhos com espaço**: sempre com aspas (`"..."`).

---

## 8) Dicas para aula estável

- Desligue Settings Sync e auto-update de extensões.  
- Mantenha só **C/C++**, **C# (ms-dotnettools.csharp)** e **PlatformIO IDE**.  
- Padronize o `.NET SDK` com `global.json`.  
- Tenha um **UNO reserva** e cabos USB testados.  
- Prepare um **script de verificação** (Momento 0) com `curl` como acima.

---

## 9) Snippets úteis

**.vscode/extensions.json** (recomendações do workspace):
```json
{
  "recommendations": [
    "ms-vscode.cpptools",
    "ms-dotnettools.csharp",
    "platformio.platformio-ide"
  ],
  "unwantedRecommendations": [
    "ms-dotnettools.csdevkit",
    "visualstudioexptteam.intellicode-csharp"
  ]
}
```

**.gitignore (na raiz)**:
```
.pio/
.DS_Store
```
---

**Pronto.** Com esse roteiro você cria o projeto PIO, compila, envia, monitora e conecta ao nosso stack .NET/Blazor, além de ter um experimento claro de latência para discutir interrupções e design de firmware responsivo.
