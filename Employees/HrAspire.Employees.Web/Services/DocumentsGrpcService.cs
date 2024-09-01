namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using HrAspire.Employees.Business.Documents;
using HrAspire.Employees.Web.Mappers;
using HrAspire.Web.Common;

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
        var documents = await this.documentsService.ListEmployeeDocumentsAsync(request.EmployeeId, request.PageNumber, request.PageSize);
        var total = await this.documentsService.GetEmployeeDocumentsCountAsync(request.EmployeeId);

        var response = new GetEmployeeDocumentsResponse { Total = total };
        foreach (var document in documents)
        {
            response.Documents.Add(document.MapToDocumentGrpcModel());
        }

        return response;
    }

    public override async Task<GetDocumentResponse> GetDocument(GetDocumentRequest request, ServerCallContext context)
    {
        var document = await this.documentsService.GetAsync(request.Id);
        if (document is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        var response = new GetDocumentResponse { Document = document.MapToDocumentDetailsGrpcModel() };
        return response;
    }

    public override async Task<GetDocumentUrlAndFileNameResponse> GetDocumentUrlAndFileName(
        GetDocumentUrlAndFileNameRequest request,
        ServerCallContext context)
    {
        var documentInfo = await this.documentsService.GetUrlAndFileNameAsync(request.Id);
        if (documentInfo is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        return new GetDocumentUrlAndFileNameResponse { Url = documentInfo.Url, FileName = documentInfo.FileName };
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

        if (createResult.IsError)
        {
            throw createResult.ToRpcException();
        }

        return new CreateDocumentResponse { Id = createResult.Data };
    }

    public override async Task<Empty> UpdateDocument(UpdateDocumentRequest request, ServerCallContext context)
    {
        var updateResult = await this.documentsService.UpdateAsync(
            request.Id,
            request.Title,
            request.Description,
            request.FileContent?.ToByteArray(),
            request.FileName);

        if (updateResult.IsError)
        {
            throw updateResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> DeleteDocument(DeleteDocumentRequest request, ServerCallContext context)
    {
        var deleteResult = await this.documentsService.DeleteAsync(request.Id);
        if (deleteResult.IsError)
        {
            throw deleteResult.ToRpcException();
        }

        return new Empty();
    }
}
