using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Forno.Ui.App>("#app");
builder.Services.AddScoped(sp => new HttpClient());

await builder.Build().RunAsync();
