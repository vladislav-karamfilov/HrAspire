namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Net;
using System.Security.Claims;

using Google.Protobuf;

using HrAspire.Employees.Web;
using HrAspire.Web.ApiGateway.Mappers;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.Documents;

using Microsoft.AspNetCore.Mvc;

public static class DocumentsEndpoints
{
    public static IEndpointConventionBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Employees/{employeeId}/Documents").RequireAuthorization();

        group.MapGet(
            "/",
            async (Documents.DocumentsClient documentsClient,
                [FromRoute] string employeeId,
                [FromQuery] int pageNumber = 0,
                [FromQuery] int pageSize = 10) =>
            {
                var documentsResponse = await documentsClient.GetEmployeeDocumentsAsync(
                    new GetEmployeeDocumentsRequest { EmployeeId = employeeId, PageNumber = pageNumber, PageSize = pageSize, });

                var documents = documentsResponse.Documents.Select(e => e.MapToResponseModel()).ToList();

                return new DocumentsResponseModel(documents, documentsResponse.Total);
            });

        group.MapPost(
            "/",
            async (Documents.DocumentsClient documentsClient,
                [FromRoute] string employeeId,
                [FromBody] DocumentCreateRequestModel model,
                ClaimsPrincipal user) =>
            {
                // TODO: Make sure the model cannot come unvalidated!!!
                var createResponse = await documentsClient.CreateDocumentAsync(new CreateDocumentRequest
                {
                    EmployeeId = employeeId,
                    Title = model.Title,
                    Description = model.Description,
                    FileContent = ByteString.CopyFrom(model.FileContent),
                    FileName = model.FileName,
                    CreatedById = user.GetId()!,
                });

                // TODO: Handle the errors with exception handling (not found, bad request and internal server error handling)
                if (string.IsNullOrWhiteSpace(createResponse.ErrorMessage))
                {
                    return Results.Created(string.Empty, createResponse.Id);
                }

                return Results.Problem(createResponse.ErrorMessage, statusCode: (int)HttpStatusCode.BadRequest);
            });

        group.MapGet(
            "/{id:int}",
            async (Documents.DocumentsClient documentsClient, [FromRoute] string employeeId, [FromRoute] int id) =>
            {
                var documentResponse = await documentsClient.GetDocumentAsync(
                    new GetDocumentRequest { Id = id, EmployeeId = employeeId });

                if (documentResponse.Document is null)
                {
                    return Results.NotFound();
                }

                var document = documentResponse.Document.MapToDetailsResponseModel();

                return Results.Ok(document);
            });

        group.MapPut(
            "/{id:int}",
            async (Documents.DocumentsClient documentsClient,
                [FromRoute] string employeeId,
                [FromRoute] int id,
                [FromBody] DocumentUpdateRequestModel model) =>
            {
                // TODO: Make sure the model cannot come unvalidated!!!
                var updateResponse = await documentsClient.UpdateDocumentAsync(new UpdateDocumentRequest
                {
                    EmployeeId = employeeId,
                    Id = id,
                    Title = model.Title,
                    Description = model.Description,
                    FileContent = model.FileContent is null ? null : ByteString.CopyFrom(model.FileContent),
                    FileName = model.FileName,
                });

                if (string.IsNullOrWhiteSpace(updateResponse.ErrorMessage))
                {
                    return Results.Ok();
                }

                return Results.Problem(updateResponse.ErrorMessage, statusCode: (int)HttpStatusCode.BadRequest);
            });

        group.MapDelete(
            "/{id:int}",
            async (Documents.DocumentsClient documentsClient, [FromRoute] string employeeId, [FromRoute] int id) =>
            {
                var deleteResponse = await documentsClient.DeleteDocumentAsync(
                    new DeleteDocumentRequest { Id = id, EmployeeId = employeeId });

                if (string.IsNullOrWhiteSpace(deleteResponse.ErrorMessage))
                {
                    return Results.Ok();
                }

                return Results.Problem(deleteResponse.ErrorMessage, statusCode: (int)HttpStatusCode.BadRequest);
            });

        return group;
    }
}
