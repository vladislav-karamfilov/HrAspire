namespace HrAspire.Web.Client.Services.Documents;

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

    public async Task<(DocumentDetailsResponseModel? Document, string? ErrorMessage)> GetDocumentAsync(int id)
    {
        var response = await this.httpClient.GetAsync($"documents/{id}");
        if (response.IsSuccessStatusCode)
        {
            var document = await response.Content.ReadFromJsonAsync<DocumentDetailsResponseModel>();
            return (document, null);
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return (null, errorMessage);
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

        var errorMessage = await response.GetErrorMessageAsync();
        return (null, errorMessage);
    }

    public async Task<string?> UpdateDocumentAsync(int id, DocumentUpdateRequestModel request)
    {
        var response = await this.httpClient.PutAsJsonAsync($"documents/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }

    public async Task<string?> DeleteDocumentAsync(int id)
    {
        var response = await this.httpClient.DeleteAsync($"documents/{id}");
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var errorMessage = await response.GetErrorMessageAsync();
        return errorMessage;
    }

    public string BuildDocumentDownloadUrl(int id) => $"{this.httpClient.BaseAddress}documents/{id}/content";
}
