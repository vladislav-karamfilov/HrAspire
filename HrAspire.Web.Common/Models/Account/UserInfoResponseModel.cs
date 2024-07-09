namespace HrAspire.Web.Common.Models.Account;

public class UserInfoResponseModel
{
    public string Id { get; set; } = default!;

    public string Email { get; set; } = default!;

    public IReadOnlyList<string> Roles { get; set; } = [];
}
