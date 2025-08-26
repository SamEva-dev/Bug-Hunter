using Bug_Hunter;
using BugHunter.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<BugSpawner>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<InvadersGameService>();
builder.Services.AddScoped<CodeBreakerService>();

await builder.Build().RunAsync();
