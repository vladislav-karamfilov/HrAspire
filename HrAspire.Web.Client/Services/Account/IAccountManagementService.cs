namespace HrAspire.Web.Client.Services.Account;

using HrAspire.Web.Common.Models.Account;

public interface IAccountManagementService
{
    Task<bool> LoginAsync(LoginRequestModel model);

    Task<bool> LogoutAsync();
}
