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

    public override async Task<ListEmployeeDocumentsResponse> ListEmployeeDocuments(
        ListEmployeeDocumentsRequest request,
        ServerCallContext context)
    {
        var documents = await this.documentsService.ListEmployeeDocumentsAsync(
            request.EmployeeId,
            request.PageNumber,
            request.PageSize,
            request.ManagerId);

        var total = await this.documentsService.GetEmployeeDocumentsCountAsync(request.EmployeeId, request.ManagerId);

        var response = new ListEmployeeDocumentsResponse { Total = total };
        foreach (var document in documents)
        {
            response.Documents.Add(document.MapToDocumentGrpcModel());
        }

        return response;
    }

    public override async Task<GetDocumentResponse> Get(GetDocumentRequest request, ServerCallContext context)
    {
        var document = await this.documentsService.GetAsync(request.Id, request.ManagerId);
        if (document is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        var response = new GetDocumentResponse { Document = document.MapToDocumentDetailsGrpcModel() };
        return response;
    }

    public override async Task<GetDocumentUrlAndFileNameResponse> GetUrlAndFileName(
        GetDocumentUrlAndFileNameRequest request,
        ServerCallContext context)
    {
        var documentInfo = await this.documentsService.GetUrlAndFileNameAsync(request.Id, request.ManagerId);
        if (documentInfo is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        return new GetDocumentUrlAndFileNameResponse { Url = documentInfo.Url, FileName = documentInfo.FileName };
    }

    public override async Task<CreateDocumentResponse> Create(CreateDocumentRequest request, ServerCallContext context)
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

    public override async Task<Empty> Update(UpdateDocumentRequest request, ServerCallContext context)
    {
        var updateResult = await this.documentsService.UpdateAsync(
            request.Id,
            request.Title,
            request.Description,
            request.FileContent?.ToByteArray(),
            request.FileName,
            request.CurrentEmployeeId);

        if (updateResult.IsError)
        {
            throw updateResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Delete(DeleteDocumentRequest request, ServerCallContext context)
    {
        var deleteResult = await this.documentsService.DeleteAsync(request.Id, request.CurrentEmployeeId);
        if (deleteResult.IsError)
        {
            throw deleteResult.ToRpcException();
        }

        return new Empty();
    }
}
