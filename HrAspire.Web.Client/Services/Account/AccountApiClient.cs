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
        var response = await this.httpClient.GetAsync("account/userInfo");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserInfoResponseModel>();
        }

        return null;
    }

    public async Task<string?> ChangePasswordAsync(ChangePasswordRequestModel model)
    {
        var response = await this.httpClient.PostAsJsonAsync("account/manage/info", model);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }
}
