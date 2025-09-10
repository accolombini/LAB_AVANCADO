using Forno.Ui.Services;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURAÇÃO DE PORTA ==========
builder.WebHost.UseUrls("http://localhost:5001");

// ========== CONFIGURAÇÃO DE SERVIÇOS BLAZOR SERVER ==========

// Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Logging
builder.Logging.AddConsole();

// Serviços HTTP
builder.Services.AddHttpClient<FornoApiService>();

// Serviços personalizados
builder.Services.AddScoped<FornoSignalRService>();

// Radzen UI Components
builder.Services.AddRadzenComponents();

// ========== CONFIGURAÇÃO DA APLICAÇÃO ==========

var app = builder.Build();

// Configurar pipeline de requisições
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Blazor Server
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

Console.WriteLine("🎯 MOMENTO 3 - SCADA Dashboard iniciado");
Console.WriteLine("🔗 Interface: http://localhost:5001");
Console.WriteLine("📡 Conectando à API: http://localhost:5002");

app.Run();
