namespace HrAspire.Web.Client.Services.Account;

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using HrAspire.Web.Client.Services.Account.Models;

public class AccountApiClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

    private readonly HttpClient httpClient;

    public AccountApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await httpClient.PostAsJsonAsync("account/login?useCookies=true", new { email, password });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LogoutAsync()
    {
        // https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#antiforgery-support
        var response = await httpClient.PostAsJsonAsync("account/logout", new object());

        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfo?> GetUserInfoAsync()
    {
        var userResponse = await httpClient.GetAsync("account/userInfo");
        if (userResponse.IsSuccessStatusCode)
        {
            return await userResponse.Content.ReadFromJsonAsync<UserInfo>(JsonSerializerOptions);
        }

        return null;
    }
}
