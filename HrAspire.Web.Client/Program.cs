using HrAspire.Web.Client.Services;
using HrAspire.Web.Services;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = await GetApiBaseUrlAsync(builder);

builder.Services
    .AddHttpClient<ApiGatewayClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

await builder.Build().RunAsync();

static async Task<string> GetApiBaseUrlAsync(WebAssemblyHostBuilder hostBuilder)
{
    using var httpClient = new HttpClient() { BaseAddress = new Uri(hostBuilder.HostEnvironment.BaseAddress) };
    return await httpClient.GetStringAsync("/apiBaseUrl");
}
