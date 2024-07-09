namespace HrAspire.Web.Client.Services.Account;

public interface IAccountManagementService
{
    Task<bool> LoginAsync(string email, string password);

    Task<bool> LogoutAsync();
}
