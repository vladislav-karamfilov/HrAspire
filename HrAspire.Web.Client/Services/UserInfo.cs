namespace HrAspire.Web.Client.Services;

public class UserInfo
{
    public string Id { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string[] Roles { get; set; } = [];
}
