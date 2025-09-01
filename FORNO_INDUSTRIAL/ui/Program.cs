using Microsoft.AspNetCore.Components.Web;using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
var b=WebAssemblyHostBuilder.CreateDefault(args);b.RootComponents.Add<App>("#app");b.Services.AddScoped(sp=>new HttpClient());await b.Build().RunAsync();
