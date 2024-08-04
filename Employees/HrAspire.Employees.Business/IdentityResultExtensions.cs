namespace HrAspire.Employees.Business;

using Microsoft.AspNetCore.Identity;

public static class IdentityResultExtensions
{
    public static string? GetFirstError(this IdentityResult result)
        => result.Errors.Select(e => e.Description).FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));
}
