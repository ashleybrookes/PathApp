using PathViewingApp.Components;
using PathViewingApp.Services;
using PathDrift.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddGrpc();
// Show detailed circuit errors in the browser during development
builder.Services.AddServerSideBlazor().AddCircuitOptions(o => o.DetailedErrors = builder.Environment.IsDevelopment());
//registering the in memory data store
builder.Services.AddSingleton<ICoordinateStore, CoordinateStore>();
builder.Services.AddSingleton<PathViewerLogic>();

// build and run application
var app = builder.Build();

// Configure the HTTP request pipeline.
// stops us going to production with an app thats not ready for production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapGrpcService<CoordinateReceiverService>();

app.Run();