namespace HrAspire.Web.Client.Services.Documents;

using System.Net;
using System.Net.Http.Json;

using HrAspire.Web.Common.Models.Documents;

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

    public async Task<DocumentDetailsResponseModel?> GetDocumentAsync(int id)
    {
        var response = await this.httpClient.GetAsync($"documents/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<DocumentDetailsResponseModel>();
        }

        return null;
    }

    public async Task<(int? DocumentId, string? ErrorMessage)> CreateDocumentAsync(
        string employeeId,
        DocumentCreateRequestModel request)
    {
        var response = await this.httpClient.PostAsJsonAsync($"employees/{employeeId}/documents", request);
        if (response.IsSuccessStatusCode)
        {
            var documentId = await response.Content.ReadFromJsonAsync<int>();
            return (documentId, null);
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

    public async Task<string?> UpdateDocumentAsync(int id, DocumentUpdateRequestModel request)
    {
        var response = await this.httpClient.PutAsJsonAsync($"documents/{id}", request);
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

    public async Task<string?> DeleteDocumentAsync(int id)
    {
        var response = await this.httpClient.DeleteAsync($"documents/{id}");
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

    public string GetDocumentDownloadUrl(int id) => $"{this.httpClient.BaseAddress}documents/{id}/content";
}
