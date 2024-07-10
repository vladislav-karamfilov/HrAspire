using HrAspire.Web.Client.Services;
using HrAspire.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.AddServiceDefaults();

builder.Services.AddApplicationServices(builder.Configuration["ApiGatewayUrl"]!);

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

app.UseStaticFiles();

app.UseAntiforgery();

app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(HrAspire.Web.Client._Imports).Assembly);

app.MapDefaultEndpoints();

app.MapGet("/apiBaseUrl", (IConfiguration configuration) => configuration["ApiGatewayUrl"]);

app.Run();
