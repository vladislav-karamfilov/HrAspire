namespace HrAspire.Web.Client.Services.Employees;

using System.Net;
using System.Net.Http.Json;

using HrAspire.Web.Common.Models.Employees;

public class EmployeesApiClient
{
    private readonly HttpClient httpClient;

    public EmployeesApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<EmployeesResponseModel> GetEmployeesAsync(int pageNumber, int pageSize)
        => this.httpClient.GetFromJsonAsync<EmployeesResponseModel>($"employees?pageNumber={pageNumber}&pageSize={pageSize}")!;

    public async Task<EmployeeDetailsResponseModel?> GetEmployeeAsync(string id)
    {
        var response = await this.httpClient.GetAsync($"employees/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<EmployeeDetailsResponseModel>();
        }

        return null;
    }

    public async Task<(string? EmployeeId, string? ErrorMessage)> CreateEmployeeAsync(EmployeeCreateRequestModel request)
    {
        var response = await this.httpClient.PostAsJsonAsync("employees", request);
        if (response.IsSuccessStatusCode)
        {
            var employeeId = await response.Content.ReadFromJsonAsync<string>();
            return (employeeId, null);
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

    public async Task<string?> UpdateEmployeeAsync(string id, EmployeeUpdateRequestModel request)
    {
        var response = await this.httpClient.PutAsJsonAsync($"employees/{id}", request);
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

    public async Task<string?> DeleteEmployeeAsync(string id)
    {
        var response = await this.httpClient.DeleteAsync($"employees/{id}");
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

    public async Task<IEnumerable<EmployeeResponseModel>> GetManagersAsync()
    {
        var response = await this.httpClient.GetAsync("employees/managers");
        if (response.IsSuccessStatusCode)
        {
            var managers = await response.Content.ReadFromJsonAsync<IEnumerable<EmployeeResponseModel>>();
            return managers!;
        }

        return [];
    }

    public async Task<IEnumerable<EmployeeResponseModel>> GetManagedEmployeesAsync()
    {
        var response = await this.httpClient.GetAsync("employees/managed");
        if (response.IsSuccessStatusCode)
        {
            var managers = await response.Content.ReadFromJsonAsync<IEnumerable<EmployeeResponseModel>>();
            return managers!;
        }

        return [];
    }
}
