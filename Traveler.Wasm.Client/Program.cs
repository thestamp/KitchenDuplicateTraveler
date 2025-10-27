using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Traveler.Wasm.Client.Services;
using Traveler.Core.Services;
using Traveler.Wasm.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Mount Routes to #app div
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

// Add HttpClient for downloading PBN files from URLs
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register Core services
builder.Services.AddScoped<FileImportService>();
builder.Services.AddScoped<BridgeScoringService>();
builder.Services.AddScoped<MatchPointsService>();

// Register WASM-specific services
builder.Services.AddScoped<Traveler.Wasm.Client.Services.TravelerService>();

await builder.Build().RunAsync();
