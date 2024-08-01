namespace HrAspire.Web.Client.Services.Documents;

using HrAspire.Web.Common.Models.Documents;
using HrAspire.Web.Common.Models.Employees;
using System.Net.Http.Json;

public class DocumentsApiClient
{
    private readonly HttpClient httpClient;

    public DocumentsApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<DocumentsResponseModel> GetEmployeeDocumentsAsync(string employeeId, int pageNumber, int pageSize)
        => this.httpClient.GetFromJsonAsync<DocumentsResponseModel>(
            $"employees/{employeeId}/documents?pageNumber={pageNumber}&pageSize={pageSize}")!;
}
