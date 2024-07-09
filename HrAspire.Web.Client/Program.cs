using HrAspire.Web.Client.Services.Account;
using HrAspire.Web.Services;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AppAuthenticationStateProvider>();
builder.Services.AddScoped(serviceProvider => (IAuthenticationService)serviceProvider.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddTransient<CookieHttpMessageHandler>();

var apiBaseUrl = await GetApiBaseUrlAsync(builder);

builder.Services
    .AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

builder.Services
    .AddHttpClient<AccountApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

await builder.Build().RunAsync();

static async Task<string> GetApiBaseUrlAsync(WebAssemblyHostBuilder hostBuilder)
{
    using var httpClient = new HttpClient() { BaseAddress = new Uri(hostBuilder.HostEnvironment.BaseAddress) };
    return await httpClient.GetStringAsync("/apiBaseUrl");
}
