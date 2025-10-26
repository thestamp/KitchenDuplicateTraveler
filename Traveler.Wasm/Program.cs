using MudBlazor.Services;
using Traveler.Core.Services;
using Traveler.Wasm.Client.Pages;
using Traveler.Wasm.Components;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient for downloading PBN files from URLs
builder.Services.AddScoped(sp => new HttpClient {  });

// Register Core services
builder.Services.AddScoped<FileImportService>();
builder.Services.AddScoped<BridgeScoringService>();
builder.Services.AddScoped<MatchPointsService>();

// Register WASM-specific services
builder.Services.AddScoped<Traveler.Wasm.Client.Services.TravelerService>();


// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Traveler.Wasm.Client._Imports).Assembly);

app.Run();
