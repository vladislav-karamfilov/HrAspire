namespace HrAspire.Web.Common.Models.SalaryRequests;

public record SalaryRequestsResponseModel(ICollection<SalaryRequestResponseModel> SalaryRequests, int Total);
