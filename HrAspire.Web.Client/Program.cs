using HrAspire.Web.Client.Services;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = await GetApiBaseUrlAsync(builder);

builder.Services.AddApplicationServices(apiBaseUrl);

await builder.Build().RunAsync();

static async Task<string> GetApiBaseUrlAsync(WebAssemblyHostBuilder hostBuilder)
{
    using var httpClient = new HttpClient() { BaseAddress = new Uri(hostBuilder.HostEnvironment.BaseAddress) };
    return await httpClient.GetStringAsync("/apiBaseUrl");
}
