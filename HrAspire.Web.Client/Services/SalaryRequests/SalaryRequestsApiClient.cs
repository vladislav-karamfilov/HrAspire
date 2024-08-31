namespace HrAspire.Web.Client.Services.SalaryRequests;

using System.Net.Http.Json;

using HrAspire.Web.Common.Models.SalaryRequests;

public class SalaryRequestsApiClient
{
    private readonly HttpClient httpClient;

    public SalaryRequestsApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<SalaryRequestsResponseModel> GetSalaryRequestsAsync(int pageNumber, int pageSize)
        => this.httpClient.GetFromJsonAsync<SalaryRequestsResponseModel>($"salaryRequests?pageNumber={pageNumber}&pageSize={pageSize}")!;
}
