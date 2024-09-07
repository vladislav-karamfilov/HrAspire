namespace HrAspire.Web.Client.Services.Employees;

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

    public async Task<(EmployeeDetailsResponseModel? Employee, string? ErrorMessage)> GetEmployeeAsync(string id)
    {
        var response = await this.httpClient.GetAsync($"employees/{id}");
        if (response.IsSuccessStatusCode)
        {
            var employee = await response.Content.ReadFromJsonAsync<EmployeeDetailsResponseModel>();
            return (employee, null);
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return (null, errorMessage);
    }

    public async Task<(string? EmployeeId, string? ErrorMessage)> CreateEmployeeAsync(EmployeeCreateRequestModel request)
    {
        var response = await this.httpClient.PostAsJsonAsync("employees", request);
        if (response.IsSuccessStatusCode)
        {
            var employeeId = await response.Content.ReadFromJsonAsync<string>();
            return (employeeId, null);
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return (null, errorMessage);
    }

    public async Task<string?> UpdateEmployeeAsync(string id, EmployeeUpdateRequestModel request)
    {
        var response = await this.httpClient.PutAsJsonAsync($"employees/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }

    public async Task<string?> DeleteEmployeeAsync(string id)
    {
        var response = await this.httpClient.DeleteAsync($"employees/{id}");
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
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
