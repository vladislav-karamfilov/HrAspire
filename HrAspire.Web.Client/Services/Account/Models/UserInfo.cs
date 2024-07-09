namespace HrAspire.Web.Client.Services.Account.Models;

public class UserInfo
{
    public string Id { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string[] Roles { get; set; } = [];
}
