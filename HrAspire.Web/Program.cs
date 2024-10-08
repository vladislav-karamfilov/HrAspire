using HrAspire.ServiceDefaults;
using HrAspire.Web.Components;
using HrAspire.Web.Services;

using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.AddServiceDefaults();

// workaround for https://github.com/dotnet/aspnetcore/issues/52063
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();
builder.Services.AddAuthorization();

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

app.UseAuthorization();

app
    .MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(HrAspire.Web.Client._Imports).Assembly);

app.MapGet("/ApiBaseUrl", (IConfiguration configuration) => configuration[EnvironmentVariableNames.ApiGatewayUrl]);

app.MapDefaultEndpoints();

app.Run();
