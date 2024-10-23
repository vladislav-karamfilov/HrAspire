namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Security.Claims;

using HrAspire.Employees.Data.Models;
using HrAspire.Web.Common.Models.Account;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public static class AccountEndpoints
{
    public static IEndpointConventionBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapIdentityApi<Employee>();

        accountGroup
            .MapGet(
                "/UserInfo",
                (ClaimsPrincipal user) => new UserInfoResponseModel(
                    user.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    user.FindFirstValue(ClaimTypes.Email)!,
                    user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()))
            .RequireAuthorization();

        accountGroup
            .MapPost(
                "/Logout",
                async (SignInManager<Employee> signInManager, [FromBody] LogoutRequestModel? model) =>
                {
                    // https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#antiforgery-support
                    if (model is not null)
                    {
                        await signInManager.SignOutAsync();

                        return Results.Ok();
                    }

                    return Results.Unauthorized();
                })
            .RequireAuthorization();

        return accountGroup;
    }
}
