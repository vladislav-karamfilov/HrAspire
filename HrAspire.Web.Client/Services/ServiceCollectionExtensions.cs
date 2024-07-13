namespace HrAspire.Web.Client.Services;

using HrAspire.Web.Client.Services.Account;
using HrAspire.Web.Client.Services.Employees;

using Microsoft.AspNetCore.Components.Authorization;

public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, string apiBaseUrl)
    {
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, AppAuthenticationStateProvider>();
        services.AddScoped(serviceProvider => (IAuthenticationService)serviceProvider.GetRequiredService<AuthenticationStateProvider>());

        services.AddTransient<CookieHttpMessageHandler>();

        services
            .AddHttpClient<AccountApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<CookieHttpMessageHandler>();

        services
            .AddHttpClient<EmployeesApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<CookieHttpMessageHandler>();

        services
            .AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl))
            .AddHttpMessageHandler<CookieHttpMessageHandler>();
    }
}
