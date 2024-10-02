using FluentValidation;

using HrAspire.Web.Client.Services;
using HrAspire.Web.Client.Services.Account;
using HrAspire.Web.Client.Services.Documents;
using HrAspire.Web.Client.Services.Employees;
using HrAspire.Web.Client.Services.SalaryRequests;
using HrAspire.Web.Client.Services.VacationRequests;
using HrAspire.Web.Common.Validators.Employees;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = await GetApiBaseUrlAsync(builder);

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AppAuthenticationStateProvider>();
builder.Services.AddScoped(serviceProvider => (IAuthenticationService)serviceProvider.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddTransient<CookieHttpMessageHandler>();

builder.Services
    .AddHttpClient<AccountApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

builder.Services
    .AddHttpClient<EmployeesApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

builder.Services
    .AddHttpClient<DocumentsApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

builder.Services
    .AddHttpClient<SalaryRequestsApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

builder.Services
    .AddHttpClient<VacationRequestsApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<CookieHttpMessageHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<EmployeeCreateRequestModelValidator>(ServiceLifetime.Singleton);

await builder.Build().RunAsync();

static async Task<string> GetApiBaseUrlAsync(WebAssemblyHostBuilder hostBuilder)
{
    using var httpClient = new HttpClient() { BaseAddress = new Uri(hostBuilder.HostEnvironment.BaseAddress) };
    return await httpClient.GetStringAsync("/ApiBaseUrl");
}
