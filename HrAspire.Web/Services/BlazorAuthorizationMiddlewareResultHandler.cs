using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

// workaround https://github.com/dotnet/aspnetcore/issues/52063#issuecomment-1817420640
public class BlazorAuthorizationMiddlewareResultHandler() : IAuthorizationMiddlewareResultHandler
{
    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
        => next(context);
}
