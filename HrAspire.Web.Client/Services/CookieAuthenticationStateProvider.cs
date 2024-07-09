namespace HrAspire.Web.Client.Services;

using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

public class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());

    private readonly AuthApiClient authApiClient;

    public CookieAuthenticationStateProvider(AuthApiClient authApiClient)
    {
        this.authApiClient = authApiClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userInfo = await this.authApiClient.GetUserInfoAsync();
            if (userInfo is not null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Email),
                    new(ClaimTypes.Email, userInfo.Email)
                };

                if (userInfo.Claims is not null)
                {
                    claims.AddRange(
                        userInfo.Claims
                            .Where(c => c.Key != ClaimTypes.Name && c.Key != ClaimTypes.Email)
                            .Select(c => new Claim(c.Key, c.Value)));
                }

                var identity = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
        }
        catch
        {
        }

        return new AuthenticationState(Unauthenticated);
    }
}
