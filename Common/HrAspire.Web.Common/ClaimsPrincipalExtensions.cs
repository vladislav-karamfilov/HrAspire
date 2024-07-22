namespace HrAspire.Web.Common;

using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static string? GetId(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
