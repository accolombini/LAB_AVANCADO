# 🔥 Laboratório – Forno Industrial (Arduino UNO + SCADA em C#) — V3

Esta versão inclui:
- **Momento 0 – Decolagem** no roteiro.
- **API** com `GET /health`, `GET /api/telemetry`, `POST /api/cmd` e **`GET /api/last`** (log simples).
- **UI** com gráfico de tendência (60s) e **rodapé** mostrando **Último comando / Resposta / Horário (UTC)**.
- **Arduino (demo)**: pasta `arduino/FornoLab_bad_loop/` com `delay(50)` para experimento de **latência comparativa**.

## Como rodar
```bash
# API
cd api
dotnet restore
DOTNET_URLS=http://localhost:5199 dotnet run

# UI
cd ../ui
dotnet restore
DOTNET_URLS=http://localhost:5289 dotnet run
```

Teste rápido:
- Health: `http://localhost:5199/health`
- UI: `http://localhost:5289` (consome a API)
- Comandos (curl / Postman): `POST /api/cmd` com corpo `"START;"`, `"MODE=AUTO;"`, `"SET_SP=185.0;"` etc.

### Usar hardware real
Edite `api/appsettings.json`:
```json
{ "UseSimulator": false, "SerialPort": "/dev/tty.usbmodemXXXX" }
```
Reinicie a API e verifique a telemetria.

### Demo de latência
1) Grave `arduino/FornoLab/FornoLab.ino` (sem bloqueio) e meça **DT_ESTOP_US**.  
2) Grave `arduino/FornoLab_bad_loop/FornoLab_bad_loop.ino` (com `delay(50)`) e repita.  
3) Compare os números e discuta impacto de código bloqueante.
