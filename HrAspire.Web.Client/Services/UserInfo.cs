namespace HrAspire.Web.Client.Services;

public class UserInfo
{
    public string Email { get; set; } = default!;

    public Dictionary<string, string> Claims { get; set; } = [];
}
