using HrAspire.Web.Services;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = await GetApiBaseUrlAsync(builder.HostEnvironment.BaseAddress);

builder.Services.AddHttpClient<ApiGatewayClient>(client => client.BaseAddress = new Uri(apiBaseUrl));

await builder.Build().RunAsync();

static async Task<string> GetApiBaseUrlAsync(string baseAddress)
{
    using var httpClient = new HttpClient() { BaseAddress = new Uri(baseAddress) };

    return await httpClient.GetStringAsync("/apiBaseUrl");
}
