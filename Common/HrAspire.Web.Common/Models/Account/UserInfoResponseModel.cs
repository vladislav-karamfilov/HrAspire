namespace HrAspire.Web.Common.Models.Account;

public record UserInfoResponseModel(string Id, string Email, IReadOnlyList<string> Roles);
