namespace HrAspire.Web.Client.Services.Account;

using System.Net.Http;
using System.Net.Http.Json;

using HrAspire.Web.Common.Models.Account;

public class AccountApiClient
{
    private readonly HttpClient httpClient;

    public AccountApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(LoginRequestModel model)
    {
        var response = await this.httpClient.PostAsJsonAsync("account/login?useCookies=true", model);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LogoutAsync()
    {
        var response = await this.httpClient.PostAsJsonAsync("account/logout", new LogoutRequestModel());

        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfoResponseModel?> GetUserInfoAsync()
    {
        var userResponse = await this.httpClient.GetAsync("account/userInfo");
        if (userResponse.IsSuccessStatusCode)
        {
            return await userResponse.Content.ReadFromJsonAsync<UserInfoResponseModel>();
        }

        return null;
    }
}
