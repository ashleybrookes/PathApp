using PathViewingApp.Components;
using PathViewingApp.Services;
using PathDrift.Shared.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel with HTTPS (HTTP/2 works natively with TLS)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5036, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(7255, o =>
    {
        o.Protocols = HttpProtocols.Http1AndHttp2;
        o.UseHttps();
    });
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddGrpc();
// Show detailed circuit errors in the browser during development
builder.Services.AddServerSideBlazor().AddCircuitOptions(o => o.DetailedErrors = builder.Environment.IsDevelopment());
//registering the in memory data store
builder.Services.AddSingleton<ICoordinateStore, CoordinateStore>();

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