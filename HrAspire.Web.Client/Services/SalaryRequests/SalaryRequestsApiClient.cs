namespace HrAspire.Web.Client.Services.SalaryRequests;

using System.Net;
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

    public async Task<(int? NewSalaryRequestId, string? ErrorMessage)> CreateSalaryRequestAsync(
        string employeeId, 
        SalaryRequestCreateRequestModel request)
    {
        var response = await this.httpClient.PostAsJsonAsync($"employees/{employeeId}/SalaryRequests", request);
        if (response.IsSuccessStatusCode)
        {
            var salaryRequestId = await response.Content.ReadFromJsonAsync<int>();
            return (salaryRequestId, null);
        }

        string? errorMessage = null;
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // TODO: Extract to an extension method on response? Handle the cases when the content is not a ProblemDetails JSON (try-catch)
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            errorMessage = problemDetails?.Detail;
        }

        return (null, errorMessage ?? Constants.UnexpectedErrorMessage);
    }
}
