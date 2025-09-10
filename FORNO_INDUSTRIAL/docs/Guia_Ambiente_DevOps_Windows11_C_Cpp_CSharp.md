# 🚀 Guia de Ambiente DevOps - Windows 11 | C/C++ & C#

## 📋 Visão Geral

Este guia apresenta a configuração completa de um ambiente de desenvolvimento DevOps no Windows 11 para projetos C/C++ e C#, focado no desenvolvimento do Forno Industrial.

---

## 🛠️ Ferramentas Essenciais

### 1. **Visual Studio Code**
- **Download**: https://code.visualstudio.com/
- **Configuração**: Extensões C/C++, C#, .NET
- **Integração**: Git, Terminal, Debugging

### 2. **.NET 9 SDK**
- **Download**: https://dotnet.microsoft.com/download
- **Versão**: .NET 9.0 (Latest)
- **Verificação**: `dotnet --version`

### 3. **Git para Windows**
- **Download**: https://git-scm.com/download/win
- **Terminal**: Git Bash integrado
- **GUI**: Git GUI e integração VS Code

### 4. **Visual Studio 2022** (Opcional)
- **Edição**: Community (gratuita)
- **Workloads**: .NET, C++ Development
- **Integração**: Azure DevOps, GitHub

---

## 🏗️ Configuração C/C++

### Compilador MinGW-w64
```bash
# Instalação via Chocolatey
choco install mingw

# Verificação
gcc --version
g++ --version
```

### PlatformIO
```bash
# Instalação via pip
pip install platformio

# Verificação
pio --version
```

### CMake
```bash
# Instalação
choco install cmake

# Verificação
cmake --version
```

---

## 🏗️ Configuração C#/.NET

### Verificação da Instalação
```bash
# Verificar SDK
dotnet --list-sdks

# Criar projeto teste
dotnet new console -n TestApp
cd TestApp
dotnet run
```

### Pacotes NuGet Essenciais
```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

---

## 🔧 Configuração do Projeto Forno Industrial

### Estrutura Recomendada
```
FORNO_INDUSTRIAL/
├── api/                    # Backend .NET
├── ui/                     # Frontend Blazor
├── firmware/               # Código embarcado
├── docs/                   # Documentação
├── tests/                  # Testes automatizados
└── .github/workflows/      # CI/CD
```

### Comandos de Build
```bash
# API
cd api
dotnet build
dotnet run

# UI
cd ui
dotnet build
dotnet run

# Firmware (PlatformIO)
cd firmware/FornoLab_PIO
pio run
```

---

## 🚀 Pipeline DevOps

### GitHub Actions (.github/workflows/ci.yml)
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-api:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    - name: Build API
      run: |
        cd api
        dotnet build --configuration Release
    - name: Test API
      run: |
        cd api
        dotnet test

  build-ui:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    - name: Build UI
      run: |
        cd ui
        dotnet build --configuration Release
```

---

## 🧪 Testes Automatizados

### Configuração xUnit
```bash
# Criar projeto de teste
dotnet new xunit -n Forno.Tests
cd Forno.Tests

# Adicionar referências
dotnet add reference ../api/Forno.Api.csproj
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

### Exemplo de Teste
```csharp
[Fact]
public async Task GetTelemetry_ReturnsOk()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/telemetry");
    
    // Assert
    response.EnsureSuccessStatusCode();
}
```

---

## 📊 Monitoramento e Logs

### Application Insights
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Serilog
```csharp
// Configuração avançada de logs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/forno-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

---

## 🔒 Segurança

### HTTPS Desenvolvimento
```bash
# Certificado de desenvolvimento
dotnet dev-certs https --trust
```

### Secrets Manager
```bash
# Inicializar secrets
dotnet user-secrets init

# Adicionar secret
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
```

---

## 📦 Containerização

### Dockerfile API
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["api/Forno.Api.csproj", "api/"]
RUN dotnet restore "api/Forno.Api.csproj"
COPY . .
WORKDIR "/src/api"
RUN dotnet build "Forno.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forno.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Forno.Api.dll"]
```

### Docker Compose
```yaml
version: '3.8'
services:
  api:
    build:
      context: .
      dockerfile: api/Dockerfile
    ports:
      - "5000:80"
    
  ui:
    build:
      context: .
      dockerfile: ui/Dockerfile
    ports:
      - "5001:80"
    depends_on:
      - api
```

---

## ⚡ Otimizações de Performance

### Build Optimization
```xml
<PropertyGroup>
  <PublishTrimmed>true</PublishTrimmed>
  <PublishSingleFile>true</PublishSingleFile>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

### Caching
```csharp
// Memory Cache
builder.Services.AddMemoryCache();

// Response Caching
builder.Services.AddResponseCaching();
```

---

## 📈 Métricas e Telemetria

### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("database", () => HealthCheckResult.Healthy())
    .AddCheck("external-service", () => HealthCheckResult.Healthy());
```

### Custom Metrics
```csharp
public class TelemetryService
{
    private readonly ILogger<TelemetryService> _logger;
    
    public void LogTemperature(double temperature)
    {
        _logger.LogInformation("Temperature: {Temperature}°C", temperature);
    }
}
```

---

## 🚀 Deploy Automatizado

### Azure DevOps Pipeline
```yaml
trigger:
- main

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Test'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: '--configuration $(buildConfiguration) --collect "Code coverage"'
```

---

## 🎯 Boas Práticas

### Código Limpo
- **SOLID Principles**
- **Clean Architecture**
- **Dependency Injection**
- **Repository Pattern**

### Controle de Versão
```bash
# Branching Strategy
git flow init

# Feature branch
git flow feature start nova-funcionalidade
git flow feature finish nova-funcionalidade

# Release
git flow release start v1.0.0
git flow release finish v1.0.0
```

### Code Review
- **Pull Requests obrigatórios**
- **Revisão por pares**
- **Testes automatizados**
- **Análise de código estático**

---

## 🔧 Troubleshooting

### Problemas Comuns

#### .NET SDK não encontrado
```bash
# Reinstalar SDK
dotnet --list-sdks
# Se vazio, reinstalar .NET 9
```

#### Port já em uso
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

#### Certificados HTTPS
```bash
# Reset certificados
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

## 📚 Recursos Adicionais

### Documentação Oficial
- **.NET**: https://docs.microsoft.com/dotnet/
- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core/
- **Blazor**: https://docs.microsoft.com/aspnet/core/blazor/
- **SignalR**: https://docs.microsoft.com/aspnet/core/signalr/

### Ferramentas Úteis
- **Postman**: Teste de APIs
- **SQL Server Management Studio**: Banco de dados
- **Redis Desktop Manager**: Cache
- **Docker Desktop**: Containerização

### Comunidade
- **Stack Overflow**: Suporte técnico
- **GitHub**: Código aberto
- **Microsoft Learn**: Treinamento
- **YouTube**: Tutoriais

---

## 🏁 Conclusão

Este ambiente DevOps fornece:
- ✅ **Desenvolvimento ágil**
- ✅ **CI/CD automatizado**
- ✅ **Qualidade de código**
- ✅ **Deploy confiável**
- ✅ **Monitoramento eficaz**

O ambiente está pronto para desenvolvimento profissional do projeto Forno Industrial com todas as melhores práticas da indústria.

---

*📅 Última atualização: Setembro 2025*  
*🏷️ Versão: Windows 11 + .NET 9*  
*👨‍💻 Configuração para Laboratório Avançado*