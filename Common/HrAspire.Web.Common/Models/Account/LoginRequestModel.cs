namespace HrAspire.Web.Common.Models.Account;

public class LoginRequestModel
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;
}
