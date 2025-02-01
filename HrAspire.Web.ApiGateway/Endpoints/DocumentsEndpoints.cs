namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Net;
using System.Security.Claims;

using Google.Protobuf;

using HrAspire.Business.Common;
using HrAspire.Employees.Web;
using HrAspire.Web.ApiGateway.Mappers;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.Documents;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

public static class DocumentsEndpoints
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1116:Split parameters should start on line after declaration",
        Justification = "Better readability.")]
    public static IEndpointConventionBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/").RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        group.MapGet(
            "/Employees/{employeeId}/Documents",
            async (Documents.DocumentsClient documentsClient,
                ClaimsPrincipal user,
                [FromRoute] string employeeId,
                [FromQuery] int pageNumber = 0,
                [FromQuery] int pageSize = 10) =>
            {
                var managerId = user.IsInRole(BusinessConstants.ManagerRole) ? user.GetId() : null;

                var documentsResponse = await documentsClient.ListEmployeeDocumentsAsync(
                    new ListEmployeeDocumentsRequest
                    {
                        EmployeeId = employeeId,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        ManagerId = managerId,
                    });

                var documents = documentsResponse.Documents.Select(e => e.MapToResponseModel()).ToList();

                return Results.Ok(new DocumentsResponseModel(documents, documentsResponse.Total));
            });

        group.MapPost(
            "/Employees/{employeeId}/Documents",
            async (Documents.DocumentsClient documentsClient,
                [FromRoute] string employeeId,
                [FromBody] DocumentCreateRequestModel model,
                ClaimsPrincipal user) =>
            {
                var createResponse = await documentsClient.CreateAsync(new CreateDocumentRequest
                {
                    EmployeeId = employeeId,
                    Title = model.Title,
                    Description = model.Description,
                    FileContent = ByteString.CopyFrom(model.FileContent),
                    FileName = model.FileName,
                    CreatedById = user.GetId()!,
                });

                return Results.Created(string.Empty, createResponse.Id);
            });

        group.MapGet(
            "/Documents/{id:int}",
            async (Documents.DocumentsClient documentsClient, [FromRoute] int id, ClaimsPrincipal user) =>
            {
                var managerId = user.IsInRole(BusinessConstants.ManagerRole) ? user.GetId() : null;

                var documentResponse = await documentsClient.GetAsync(new GetDocumentRequest { Id = id, ManagerId = managerId });

                var document = documentResponse.Document.MapToDetailsResponseModel();

                return Results.Ok(document);
            });

        group.MapPut(
            "/Documents/{id:int}",
            async (Documents.DocumentsClient documentsClient,
                [FromRoute] int id,
                [FromBody] DocumentUpdateRequestModel model,
                ClaimsPrincipal user) =>
            {
                await documentsClient.UpdateAsync(new UpdateDocumentRequest
                {
                    Id = id,
                    Title = model.Title,
                    Description = model.Description,
                    FileContent = model.FileContent is null ? null : ByteString.CopyFrom(model.FileContent),
                    FileName = model.FileName,
                    CurrentEmployeeId = user.GetId()!,
                });

                return Results.Ok();
            });

        group.MapDelete(
            "/Documents/{id:int}",
            async (Documents.DocumentsClient documentsClient, [FromRoute] int id, ClaimsPrincipal user) =>
            {
                await documentsClient.DeleteAsync(new DeleteDocumentRequest { Id = id, CurrentEmployeeId = user.GetId()!, });

                return Results.Ok();
            });

        group.MapGet(
            "/Documents/{id:int}/Content",
            async (Documents.DocumentsClient documentsClient,
                HttpClient httpClient,
                HttpResponse response,
                ClaimsPrincipal user,
                [FromRoute] int id) =>
            {
                var managerId = user.IsInRole(BusinessConstants.ManagerRole) ? user.GetId() : null;

                var documentInfo = await documentsClient.GetUrlAndFileNameAsync(
                    new GetDocumentUrlAndFileNameRequest { Id = id, ManagerId = managerId });

                var documentContentResponse = await httpClient.GetAsync(documentInfo.Url, HttpCompletionOption.ResponseHeadersRead);
                if (!documentContentResponse.IsSuccessStatusCode)
                {
                    if (documentContentResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await response.WriteAsync("Error downloading document. Please try again later.");
                    }

                    return;
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.Headers.ContentLength = documentContentResponse.Content.Headers.ContentLength;
                response.Headers.LastModified = documentContentResponse.Content.Headers.LastModified?.ToString("R");
                response.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment") { FileName = documentInfo.FileName }.ToString();

                response.Headers.ContentType = documentContentResponse.Content.Headers.ContentType?.ToString()
                    ?? "application/octet-stream";

                using var fileStream = await documentContentResponse.Content.ReadAsStreamAsync();
                await fileStream.CopyToAsync(response.Body);
            });

        return group;
    }
}
