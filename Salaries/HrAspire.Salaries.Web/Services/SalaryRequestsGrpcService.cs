namespace HrAspire.Salaries.Web.Services;

using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using HrAspire.Salaries.Business.SalaryRequests;
using HrAspire.Web.Common;

public class SalaryRequestsGrpcService : SalaryRequests.SalaryRequestsBase
{
    private readonly ISalaryRequestsService salaryRequestsService;

    public SalaryRequestsGrpcService(ISalaryRequestsService salaryRequestsService)
    {
        this.salaryRequestsService = salaryRequestsService;
    }

    public override Task<GetSalaryRequestsResponse> GetSalaryRequests(GetSalaryRequestsRequest request, ServerCallContext context)
    {
        return base.GetSalaryRequests(request, context);
    }

    public override Task<GetSalaryRequestResponse> GetSalaryRequest(GetSalaryRequestRequest request, ServerCallContext context)
    {
        return base.GetSalaryRequest(request, context);
    }

    public override async Task<CreateSalaryRequestResponse> CreateSalaryRequest(
        CreateSalaryRequestRequest request,
        ServerCallContext context)
    {
        var createResult = await this.salaryRequestsService.CreateAsync(
            request.EmployeeId,
            request.NewSalary,
            request.Notes,
            request.CreatedById);

        if (createResult.IsError)
        {
            throw createResult.ToRpcException();
        }

        return new CreateSalaryRequestResponse { Id = createResult.Data };
    }

    public override async Task<Empty> UpdateSalaryRequest(UpdateSalaryRequestRequest request, ServerCallContext context)
    {
        var updateResult = await this.salaryRequestsService.UpdateAsync(request.Id, request.NewSalary, request.Notes);
        if (updateResult.IsError)
        {
            throw updateResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> DeleteSalaryRequest(DeleteSalaryRequestRequest request, ServerCallContext context)
    {
        var deleteResult = await this.salaryRequestsService.DeleteAsync(request.Id);
        if (deleteResult.IsError)
        {
            throw deleteResult.ToRpcException();
        }

        return new Empty();
    }
}
