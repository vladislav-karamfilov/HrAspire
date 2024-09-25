namespace HrAspire.Vacations.Web.Services;

using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using HrAspire.Vacations.Business.VacationRequests;
using HrAspire.Vacations.Data.Models;
using HrAspire.Vacations.Web.Mappers;
using HrAspire.Web.Common;

public class VacationRequestsGrpcService : VacationRequests.VacationRequestsBase
{
    private readonly IVacationRequestsService vacationRequestsService;

    public VacationRequestsGrpcService(IVacationRequestsService vacationRequestsService)
    {
        this.vacationRequestsService = vacationRequestsService;
    }

    public override async Task<ListVacationRequestsResponse> ListEmployeeVacationRequests(
        ListEmployeeVacationRequestsRequest request,
        ServerCallContext context)
    {
        var vacationRequests = await this.vacationRequestsService.ListEmployeeVacationRequestsAsync(
            request.EmployeeId,
            request.PageNumber,
            request.PageSize);

        var total = await this.vacationRequestsService.GetEmployeeVacationRequestsCountAsync(request.EmployeeId);

        var response = new ListVacationRequestsResponse { Total = total };
        foreach (var vacationRequest in vacationRequests)
        {
            response.VacationRequests.Add(vacationRequest.MapToVacationRequestGrpcModel());
        }

        return response;
    }

    public override async Task<GetVacationRequestResponse> Get(GetVacationRequestRequest request, ServerCallContext context)
    {
        var vacationRequest = await this.vacationRequestsService.GetAsync(request.Id);
        if (vacationRequest is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        var response = new GetVacationRequestResponse { VacationRequest = vacationRequest.MapToVacationRequestDetailsGrpcModel() };
        return response;
    }

    public override async Task<CreateVacationRequestResponse> Create(CreateVacationRequestRequest request, ServerCallContext context)
    {
        var createResult = await this.vacationRequestsService.CreateAsync(
            request.EmployeeId,
            (VacationRequestType)(int)request.Type,
            request.FromDate.ToDateOnly(),
            request.ToDate.ToDateOnly(),
            request.Notes);

        if (createResult.IsError)
        {
            throw createResult.ToRpcException();
        }

        return new CreateVacationRequestResponse { Id = createResult.Data };
    }

    public override async Task<Empty> Update(UpdateVacationRequestRequest request, ServerCallContext context)
    {
        var updateResult = await this.vacationRequestsService.UpdateAsync(
            request.Id,
            (VacationRequestType)(int)request.Type,
            request.FromDate.ToDateOnly(),
            request.ToDate.ToDateOnly(),
            request.Notes);

        if (updateResult.IsError)
        {
            throw updateResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Delete(DeleteVacationRequestRequest request, ServerCallContext context)
    {
        var deleteResult = await this.vacationRequestsService.DeleteAsync(request.Id);
        if (deleteResult.IsError)
        {
            throw deleteResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Approve(ChangeStatusOfVacationRequestRequest request, ServerCallContext context)
    {
        var result = await this.vacationRequestsService.ApproveAsync(request.Id, request.CurrentEmployeeId);
        if (result.IsError)
        {
            throw result.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Reject(ChangeStatusOfVacationRequestRequest request, ServerCallContext context)
    {
        var result = await this.vacationRequestsService.RejectAsync(request.Id, request.CurrentEmployeeId);
        if (result.IsError)
        {
            throw result.ToRpcException();
        }

        return new Empty();
    }
}
