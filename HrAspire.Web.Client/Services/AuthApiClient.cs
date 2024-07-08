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

    public async Task<IEnumerable<string>> RegisterAsync(string email, string password)
    {
        var response = await this.httpClient.PostAsJsonAsync("register", new { email, password });
        if (response.IsSuccessStatusCode)
        {
            return [];
        }

        var errors = new List<string>();

        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonDocument.Parse(responseContent);
        var errorList = problemDetails.RootElement.GetProperty("errors");

        foreach (var errorEntry in errorList.EnumerateObject())
        {
            if (errorEntry.Value.ValueKind == JsonValueKind.String)
            {
                errors.Add(errorEntry.Value.GetString()!);
            }
            else if (errorEntry.Value.ValueKind == JsonValueKind.Array)
            {
                errors.AddRange(
                    errorEntry.Value
                        .EnumerateArray()
                        .Select(e => e.GetString() ?? string.Empty)
                        .Where(e => !string.IsNullOrEmpty(e)));
            }
        }

        if (errors.Any())
        {
            return errors;
        }

        return ["An unexpected error has occurred. Please try again later."];
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await this.httpClient.PostAsJsonAsync("login?useCookies=true", new { email, password });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LogoutAsync()
    {
        // https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-identity?view=aspnetcore-8.0#antiforgery-support
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

        return await userResponse.Content.ReadFromJsonAsync<UserInfo?>();
    }
}
