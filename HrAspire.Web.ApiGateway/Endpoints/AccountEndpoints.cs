namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Security.Claims;

using HrAspire.Employees.Data.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public static class AccountEndpoints
{
    public static IEndpointConventionBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapIdentityApi<Employee>();

        accountGroup
            .MapGet("/UserInfo", (ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = user.FindFirstValue(ClaimTypes.Email);
                var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                return new { id = userId, email = userEmail, roles }; // TODO: extract model
            })
            .RequireAuthorization();

        // https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#antiforgery-support
        accountGroup
            .MapPost("/Logout", async (SignInManager<Employee> signInManager, [FromBody] object empty) =>
            {
                if (empty is not null)
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
