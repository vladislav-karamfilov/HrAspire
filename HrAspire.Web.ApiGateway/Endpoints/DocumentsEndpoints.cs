namespace HrAspire.Web.ApiGateway.Endpoints;

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
            (Documents.DocumentsClient documentsClient,
                [FromRoute] string employeeId,
                [FromQuery] int pageNumber = 0,
                [FromQuery] int pageSize = 10)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var documentsResponse = await documentsClient.GetEmployeeDocumentsAsync(
                        new GetEmployeeDocumentsRequest { EmployeeId = employeeId, PageNumber = pageNumber, PageSize = pageSize, });

                    var documents = documentsResponse.Documents.Select(e => e.MapToResponseModel()).ToList();

                    return Results.Ok(new DocumentsResponseModel(documents, documentsResponse.Total));
                }));

        group.MapPost(
            "/",
            (Documents.DocumentsClient documentsClient,
                [FromRoute] string employeeId,
                [FromBody] DocumentCreateRequestModel model,
                ClaimsPrincipal user)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
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

                    return Results.Created(string.Empty, createResponse.Id);
                }));

        group.MapGet(
            "/{id:int}",
            (Documents.DocumentsClient documentsClient, [FromRoute] string employeeId, [FromRoute] int id)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var documentResponse = await documentsClient.GetDocumentAsync(
                        new GetDocumentRequest { Id = id, EmployeeId = employeeId });

                    var document = documentResponse.Document.MapToDetailsResponseModel();

                    return Results.Ok(document);
                }));

        group.MapPut(
            "/{id:int}",
            (Documents.DocumentsClient documentsClient,
                [FromRoute] string employeeId,
                [FromRoute] int id,
                [FromBody] DocumentUpdateRequestModel model)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    // TODO: Make sure the model cannot come unvalidated!!!
                    await documentsClient.UpdateDocumentAsync(new UpdateDocumentRequest
                    {
                        EmployeeId = employeeId,
                        Id = id,
                        Title = model.Title,
                        Description = model.Description,
                        FileContent = model.FileContent is null ? null : ByteString.CopyFrom(model.FileContent),
                        FileName = model.FileName,
                    });

                    return Results.Ok();
                }));

        group.MapDelete(
            "/{id:int}",
            (Documents.DocumentsClient documentsClient, [FromRoute] string employeeId, [FromRoute] int id)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    await documentsClient.DeleteDocumentAsync(new DeleteDocumentRequest { Id = id, EmployeeId = employeeId });

                    return Results.Ok();
                }));

        return group;
    }
}
