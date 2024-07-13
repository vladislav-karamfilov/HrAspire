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

    public Task<EmployeesPageResponseModel> GetEmployeesPageAsync(int pageNumber, int pageSize)
        => this.httpClient.GetFromJsonAsync<EmployeesPageResponseModel>($"employees?pageNumber={pageNumber}&pageSize={pageSize}")!;

    public Task<EmployeeDetailsResponseModel?> GetEmployeeDetailsAsync(string id)
        => this.httpClient.GetFromJsonAsync<EmployeeDetailsResponseModel>($"employees/{id}");
}
