namespace HrAspire.Web.Client.Services;

using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public class AuthApiClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

    private readonly HttpClient httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await this.httpClient.PostAsJsonAsync("login?useCookies=true", new { email, password });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LogoutAsync()
    {
        // https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#antiforgery-support
        const string EmptyObjectString = "{}";
        var emptyContent = new StringContent(EmptyObjectString, Encoding.UTF8, "application/json");

        var response = await this.httpClient.PostAsync("logout", emptyContent);

        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfo?> GetUserInfoAsync()
    {
        var userResponse = await this.httpClient.GetAsync("manage/info");
        if (!userResponse.IsSuccessStatusCode)
        {
            return null;
        }

        return await userResponse.Content.ReadFromJsonAsync<UserInfo?>(JsonSerializerOptions);
    }
}
