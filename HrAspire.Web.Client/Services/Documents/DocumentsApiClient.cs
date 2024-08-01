﻿namespace HrAspire.Web.Client.Services.Documents;

using System.Net;
using System.Net.Http.Json;

using HrAspire.Web.Common.Models.Documents;
using HrAspire.Web.Common.Models.Employees;

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

    public async Task<DocumentDetailsResponseModel?> GetDocumentAsync(int id, string employeeId)
    {
        var response = await this.httpClient.GetAsync($"employees/{employeeId}/documents/{id}");
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

    public async Task<string?> UpdateDocumentAsync(int id, string employeeId, DocumentUpdateRequestModel request)
    {
        var response = await this.httpClient.PostAsJsonAsync($"employees/{employeeId}/documents/{id}", request);
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