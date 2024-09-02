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

    public Task<SalaryRequestsResponseModel> GetEmployeeSalaryRequestsAsync(string employeeId, int pageNumber, int pageSize)
        => this.httpClient.GetFromJsonAsync<SalaryRequestsResponseModel>(
            $"employees/{employeeId}/salaryRequests?pageNumber={pageNumber}&pageSize={pageSize}")!;

    public async Task<SalaryRequestDetailsResponseModel?> GetSalaryRequestAsync(int id)
    {
        var response = await this.httpClient.GetAsync($"salaryRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SalaryRequestDetailsResponseModel>();
        }

        return null;
    }

    public async Task<(int? NewSalaryRequestId, string? ErrorMessage)> CreateSalaryRequestAsync(
        string employeeId,
        SalaryRequestCreateRequestModel request)
    {
        var response = await this.httpClient.PostAsJsonAsync($"employees/{employeeId}/salaryRequests", request);
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

    public async Task<string?> UpdateSalaryRequestAsync(int id, SalaryRequestUpdateRequestModel request)
    {
        var response = await this.httpClient.PutAsJsonAsync($"salaryRequests/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        string? errorMessage = null;
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // TODO: Extract to an extension method on response? Handle the cases when the content is not a ProblemDetails JSON (try-catch)
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            errorMessage = problemDetails?.Detail;
        }

        return errorMessage ?? Constants.UnexpectedErrorMessage;
    }

    public async Task<string?> DeleteSalaryRequestAsync(int id)
    {
        var response = await this.httpClient.DeleteAsync($"salaryRequests/{id}");
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        string? errorMessage = null;
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            errorMessage = problemDetails?.Detail;
        }

        return errorMessage ?? Constants.UnexpectedErrorMessage;
    }

    public async Task<string?> ApproveSalaryRequestAsync(int id)
    {
        var response = await this.httpClient.PostAsync($"salaryRequests/{id}/approval", content: null);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        string? errorMessage = null;
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // TODO: Extract to an extension method on response? Handle the cases when the content is not a ProblemDetails JSON (try-catch)
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            errorMessage = problemDetails?.Detail;
        }

        return errorMessage ?? Constants.UnexpectedErrorMessage;
    }

    public async Task<string?> RejectSalaryRequestAsync(int id)
    {
        var response = await this.httpClient.PostAsync($"salaryRequests/{id}/rejection", content: null);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        string? errorMessage = null;
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // TODO: Extract to an extension method on response? Handle the cases when the content is not a ProblemDetails JSON (try-catch)
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            errorMessage = problemDetails?.Detail;
        }

        return errorMessage ?? Constants.UnexpectedErrorMessage;
    }
}
