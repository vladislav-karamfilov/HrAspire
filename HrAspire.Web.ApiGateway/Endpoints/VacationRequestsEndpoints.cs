namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Security.Claims;

using HrAspire.Vacations.Web;
using HrAspire.Web.ApiGateway.Mappers;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.VacationRequests;

using Microsoft.AspNetCore.Mvc;

public static class VacationRequestsEndpoints
{
    public static IEndpointConventionBuilder MapVacationRequestsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/").RequireAuthorization();

        // TODO: ?
        //group.MapGet(
        //    "/SalaryRequests",
        //    (SalaryRequests.SalaryRequestsClient salaryRequestsClient,
        //        [FromQuery] int pageNumber = 0,
        //        [FromQuery] int pageSize = 10)
        //        => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
        //        {
        //            var salaryRequestsResponse = await salaryRequestsClient.ListAsync(
        //                new ListSalaryRequestsRequest { PageNumber = pageNumber, PageSize = pageSize });

        //            var salaryRequests = salaryRequestsResponse.SalaryRequests.Select(e => e.MapToResponseModel()).ToList();

        //            return Results.Ok(new SalaryRequestsResponseModel(salaryRequests, salaryRequestsResponse.Total));
        //        }));

        group.MapGet(
            "/Employees/{employeeId}/VacationRequests",
            (VacationRequests.VacationRequestsClient vacationRequestsClient,
                [FromRoute] string employeeId,
                [FromQuery] int pageNumber = 0,
                [FromQuery] int pageSize = 10)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var vacationRequestsResponse = await vacationRequestsClient.ListEmployeeVacationRequestsAsync(
                        new ListEmployeeVacationRequestsRequest
                        {
                            EmployeeId = employeeId,
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                        });

                    var vacationRequests = vacationRequestsResponse.VacationRequests.Select(e => e.MapToResponseModel()).ToList();

                    return Results.Ok(new VacationRequestsResponseModel(vacationRequests, vacationRequestsResponse.Total));
                }));

        group.MapPost(
            "/VacationRequests",
            (VacationRequests.VacationRequestsClient vacationRequestsClient,
                [FromBody] VacationRequestCreateRequestModel model,
                ClaimsPrincipal user)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var createResponse = await vacationRequestsClient.CreateAsync(new CreateVacationRequestRequest
                    {
                        EmployeeId = user.GetId()!,
                        Type = (VacationRequestType)(int)model.Type.GetValueOrDefault(),
                        FromDate = model.FromDate.GetValueOrDefault().ToTimestamp(),
                        ToDate = model.ToDate.GetValueOrDefault().ToTimestamp(),
                        Notes = model.Notes,
                    });

                    return Results.Created(string.Empty, createResponse.Id);
                }));

        group.MapGet(
            "/VacationRequests/{id:int}",
            (VacationRequests.VacationRequestsClient vacationRequestsClient, [FromRoute] int id)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var vacationRequestResponse = await vacationRequestsClient.GetAsync(new GetVacationRequestRequest { Id = id });

                    var vacationRequest = vacationRequestResponse.VacationRequest.MapToDetailsResponseModel();

                    return Results.Ok(vacationRequest);
                }));

        group.MapPut(
            "/VacationRequests/{id:int}",
            (VacationRequests.VacationRequestsClient vacationRequestsClient,
                [FromRoute] int id,
                [FromBody] VacationRequestUpdateRequestModel model)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var updateResponse = await vacationRequestsClient.UpdateAsync(
                        new UpdateVacationRequestRequest
                        {
                            Id = id,
                            Type = (VacationRequestType)(int)model.Type.GetValueOrDefault(),
                            FromDate = model.FromDate.ToTimestamp(),
                            ToDate = model.ToDate.ToTimestamp(),
                            Notes = model.Notes,
                        });

                    return Results.Ok();
                }));

        group.MapDelete(
            "/VacationRequests/{id:int}",
            (VacationRequests.VacationRequestsClient vacationRequestsClient, [FromRoute] int id)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    await vacationRequestsClient.DeleteAsync(new DeleteVacationRequestRequest { Id = id });

                    return Results.Ok();
                }));
        // TODO: .RequireAuthorization(Constants.ManagerAuthPolicyName);

        group
            .MapPost(
                "/VacationRequests/{id:int}/Approval",
                (VacationRequests.VacationRequestsClient vacationRequestsClient, [FromRoute] int id, ClaimsPrincipal user)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        await vacationRequestsClient.ApproveAsync(
                            new ChangeStatusOfVacationRequestRequest { Id = id, CurrentEmployeeId = user.GetId()! });

                        return Results.Ok();
                    }))
            .RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        group
            .MapPost(
                "/VacationRequests/{id:int}/Rejection",
                (VacationRequests.VacationRequestsClient vacationRequestsClient, [FromRoute] int id, ClaimsPrincipal user)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        await vacationRequestsClient.RejectAsync(
                            new ChangeStatusOfVacationRequestRequest { Id = id, CurrentEmployeeId = user.GetId()! });

                        return Results.Ok();
                    }))
            .RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        return group;
    }
}
