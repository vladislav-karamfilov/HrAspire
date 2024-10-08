﻿namespace HrAspire.Salaries.Web.Services;

using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using HrAspire.Salaries.Business.SalaryRequests;
using HrAspire.Salaries.Web.Mappers;
using HrAspire.Web.Common;

public class SalaryRequestsGrpcService : SalaryRequests.SalaryRequestsBase
{
    private readonly ISalaryRequestsService salaryRequestsService;

    public SalaryRequestsGrpcService(ISalaryRequestsService salaryRequestsService)
    {
        this.salaryRequestsService = salaryRequestsService;
    }

    public override async Task<ListSalaryRequestsResponse> List(ListSalaryRequestsRequest request, ServerCallContext context)
    {
        var salaryRequests = await this.salaryRequestsService.ListAsync(request.PageNumber, request.PageSize);
        var total = await this.salaryRequestsService.GetCountAsync();

        var response = new ListSalaryRequestsResponse { Total = total };
        foreach (var salaryRequest in salaryRequests)
        {
            response.SalaryRequests.Add(salaryRequest.MapToSalaryRequestGrpcModel());
        }

        return response;
    }

    public override async Task<ListSalaryRequestsResponse> ListEmployeeSalaryRequests(
        ListEmployeeSalaryRequestsRequest request,
        ServerCallContext context)
    {
        var salaryRequests = await this.salaryRequestsService.ListEmployeeSalaryRequestsAsync(
            request.EmployeeId,
            request.PageNumber,
            request.PageSize,
            request.ManagerId);

        var total = await this.salaryRequestsService.GetEmployeeSalaryRequestsCountAsync(request.EmployeeId, request.ManagerId);

        var response = new ListSalaryRequestsResponse { Total = total };
        foreach (var salaryRequest in salaryRequests)
        {
            response.SalaryRequests.Add(salaryRequest.MapToSalaryRequestGrpcModel());
        }

        return response;
    }

    public override async Task<GetSalaryRequestResponse> Get(GetSalaryRequestRequest request, ServerCallContext context)
    {
        var salaryRequest = await this.salaryRequestsService.GetAsync(request.Id, request.ManagerId);
        if (salaryRequest is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        var response = new GetSalaryRequestResponse { SalaryRequest = salaryRequest.MapToSalaryRequestDetailsGrpcModel() };
        return response;
    }

    public override async Task<CreateSalaryRequestResponse> Create(CreateSalaryRequestRequest request, ServerCallContext context)
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

    public override async Task<Empty> Update(UpdateSalaryRequestRequest request, ServerCallContext context)
    {
        var updateResult = await this.salaryRequestsService.UpdateAsync(
            request.Id,
            request.NewSalary,
            request.Notes,
            request.CurrentEmployeeId);

        if (updateResult.IsError)
        {
            throw updateResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Delete(DeleteSalaryRequestRequest request, ServerCallContext context)
    {
        var deleteResult = await this.salaryRequestsService.DeleteAsync(request.Id, request.CurrentEmployeeId);
        if (deleteResult.IsError)
        {
            throw deleteResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Approve(ChangeStatusOfSalaryRequestRequest request, ServerCallContext context)
    {
        var result = await this.salaryRequestsService.ApproveAsync(request.Id, request.CurrentEmployeeId);
        if (result.IsError)
        {
            throw result.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Reject(ChangeStatusOfSalaryRequestRequest request, ServerCallContext context)
    {
        var result = await this.salaryRequestsService.RejectAsync(request.Id, request.CurrentEmployeeId);
        if (result.IsError)
        {
            throw result.ToRpcException();
        }

        return new Empty();
    }
}
