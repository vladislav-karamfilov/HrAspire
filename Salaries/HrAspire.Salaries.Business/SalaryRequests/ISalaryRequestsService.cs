namespace HrAspire.Salaries.Business.SalaryRequests;

public interface ISalaryRequestsService
{
    Task<IEnumerable<SalaryRequestServiceModel>> GetSalaryRequestsAsync();
}
