namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using Grpc.Core;

using HrAspire.Employees.Business.Documents;
using HrAspire.Employees.Web.Mappers;

public class DocumentsGrpcService : Documents.DocumentsBase
{
    private readonly IDocumentsService documentsService;

    public DocumentsGrpcService(IDocumentsService documentsService)
    {
        this.documentsService = documentsService;
    }

    public override async Task<GetEmployeeDocumentsResponse> GetEmployeeDocuments(
        GetEmployeeDocumentsRequest request,
        ServerCallContext context)
    {
        var documents = await this.documentsService.GetEmployeeDocumentsAsync(request.EmployeeId, request.PageNumber, request.PageSize);
        var total = await this.documentsService.GetEmployeeDocumentsCountAsync(request.EmployeeId);

        var response = new GetEmployeeDocumentsResponse { Total = total };
        foreach (var document in documents)
        {
            response.Documents.Add(document.MapToDocumentModel());
        }

        return response;
    }

    public override async Task<GetDocumentResponse> GetDocument(GetDocumentRequest request, ServerCallContext context)
    {
        var document = await this.documentsService.GetDocumentAsync(request.Id, request.EmployeeId);

        var response = new GetDocumentResponse { Document = document?.MapToDocumentDetails() };

        return response;
    }

    public override async Task<CreateDocumentResponse> CreateDocument(CreateDocumentRequest request, ServerCallContext context)
    {
        var createResult = await this.documentsService.CreateAsync(
            request.EmployeeId,
            request.Title,
            request.Description,
            request.FileContent.ToByteArray(),
            request.FileName,
            request.CreatedById);

        return new CreateDocumentResponse { Id = createResult.Data, ErrorMessage = createResult.ErrorMessage };
    }

    public override async Task<UpdateDocumentResponse> UpdateDocument(UpdateDocumentRequest request, ServerCallContext context)
    {
        var updateResult = await this.documentsService.UpdateAsync(
            request.Id,
            request.EmployeeId,
            request.Title,
            request.Description,
            request.FileContent?.ToByteArray(),
            request.FileName);

        return new UpdateDocumentResponse { ErrorMessage = updateResult.ErrorMessage };
    }

    public override async Task<DeleteDocumentResponse> DeleteDocument(DeleteDocumentRequest request, ServerCallContext context)
    {
        var deleteResult = await this.documentsService.DeleteAsync(request.Id, request.EmployeeId);

        return new DeleteDocumentResponse { ErrorMessage = deleteResult.ErrorMessage };
    }
}
