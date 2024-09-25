namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Security.Claims;

using HrAspire.Salaries.Web;
using HrAspire.Vacations.Web;
using HrAspire.Web.ApiGateway.Mappers;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.SalaryRequests;
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
                    // TODO: Make sure the model cannot come unvalidated!!!
                    var createResponse = await vacationRequestsClient.CreateAsync(new CreateVacationRequestRequest
                    {
                        EmployeeId = user.GetId()!,
                        Type = (VacationRequestType)(int)model.Type!,
                        FromDate = model.FromDate.ToTimestamp(),
                        ToDate = model.ToDate.ToTimestamp(),
                        Notes = model.Notes,
                    });

                    return Results.Created(string.Empty, createResponse.Id);
                }));

        //group
        //    .MapGet(
        //        "/SalaryRequests/{id:int}",
        //        (SalaryRequests.SalaryRequestsClient salaryRequestsClient, [FromRoute] int id)
        //            => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
        //            {
        //                var salaryRequestResponse = await salaryRequestsClient.GetAsync(new GetSalaryRequestRequest { Id = id });

        //                var salaryRequest = salaryRequestResponse.SalaryRequest.MapToDetailsResponseModel();

        //                return Results.Ok(salaryRequest);
        //            }))
        //    .RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        //group
        //    .MapPut(
        //        "/SalaryRequests/{id:int}",
        //        (SalaryRequests.SalaryRequestsClient salaryRequestsClient,
        //            [FromRoute] int id,
        //            [FromBody] SalaryRequestUpdateRequestModel model)
        //            => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
        //            {
        //                // TODO: Make sure the model cannot come unvalidated!!!
        //                var updateResponse = await salaryRequestsClient.UpdateAsync(
        //                    new UpdateSalaryRequestRequest { Id = id, NewSalary = model.NewSalary, Notes = model.Notes, });

        //                return Results.Ok();
        //            }));
        //// TODO: .RequireAuthorization(Constants.ManagerAuthPolicyName);

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
            .RequireAuthorization(Constants.ManagerAuthPolicyName);

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
            .RequireAuthorization(Constants.ManagerAuthPolicyName);

        return group;
    }
}
