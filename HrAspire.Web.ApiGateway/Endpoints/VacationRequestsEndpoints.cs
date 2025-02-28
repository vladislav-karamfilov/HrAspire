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

        group.MapGet("/Employees/{employeeId}/VacationRequests", GetEmployeeVacationRequestsAsync);

        group.MapGet("/VacationRequests/{id:int}", GetVacationRequestAsync);

        group.MapPost("/VacationRequests", CreateVacationRequestAsync);

        group.MapPut("/VacationRequests/{id:int}", UpdateVacationRequestAsync);

        group.MapDelete("/VacationRequests/{id:int}", DeleteVacationRequestAsync);

        group
            .MapPost("/VacationRequests/{id:int}/Approval", ApproveVacationRequestAsync)
            .RequireAuthorization(Constants.ManagerAuthPolicyName);

        group
            .MapPost("/VacationRequests/{id:int}/Rejection", RejectVacationRequestAsync)
            .RequireAuthorization(Constants.ManagerAuthPolicyName);

        return group;
    }

    private static async Task<IResult> GetEmployeeVacationRequestsAsync(
        VacationRequests.VacationRequestsClient vacationRequestsClient,
        ClaimsPrincipal user,
        [FromRoute] string employeeId,
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10)
    {
        var vacationRequestsResponse = await vacationRequestsClient.ListEmployeeVacationRequestsAsync(
            new ListEmployeeVacationRequestsRequest
            {
                EmployeeId = employeeId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                CurrentEmployeeId = user.GetId(),
            });

        var vacationRequests = vacationRequestsResponse.VacationRequests.Select(e => e.MapToResponseModel()).ToList();

        return Results.Ok(new VacationRequestsResponseModel(vacationRequests, vacationRequestsResponse.Total));
    }

    private static async Task<IResult> GetVacationRequestAsync(
        VacationRequests.VacationRequestsClient vacationRequestsClient,
        [FromRoute] int id,
        ClaimsPrincipal user)
    {
        var vacationRequestResponse = await vacationRequestsClient.GetAsync(
            new GetVacationRequestRequest { Id = id, CurrentEmployeeId = user.GetId() });

        var vacationRequest = vacationRequestResponse.VacationRequest.MapToDetailsResponseModel();

        return Results.Ok(vacationRequest);
    }

    private static async Task<IResult> CreateVacationRequestAsync(
        VacationRequests.VacationRequestsClient vacationRequestsClient,
        [FromBody] VacationRequestCreateRequestModel model,
        ClaimsPrincipal user)
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
    }

    private static async Task<IResult> UpdateVacationRequestAsync(
        VacationRequests.VacationRequestsClient vacationRequestsClient,
        [FromRoute] int id,
        [FromBody] VacationRequestUpdateRequestModel model,
        ClaimsPrincipal user)
    {
        await vacationRequestsClient.UpdateAsync(new UpdateVacationRequestRequest
        {
            Id = id,
            Type = (VacationRequestType)(int)model.Type.GetValueOrDefault(),
            FromDate = model.FromDate.ToTimestamp(),
            ToDate = model.ToDate.ToTimestamp(),
            Notes = model.Notes,
            CurrentEmployeeId = user.GetId(),
        });

        return Results.Ok();
    }

    private static async Task<IResult> DeleteVacationRequestAsync(
        VacationRequests.VacationRequestsClient vacationRequestsClient,
        [FromRoute] int id,
        ClaimsPrincipal user)
    {
        await vacationRequestsClient.DeleteAsync(new DeleteVacationRequestRequest { Id = id, CurrentEmployeeId = user.GetId() });

        return Results.Ok();
    }

    private static async Task<IResult> ApproveVacationRequestAsync(
        VacationRequests.VacationRequestsClient vacationRequestsClient,
        [FromRoute] int id,
        ClaimsPrincipal user)
    {
        await vacationRequestsClient.ApproveAsync(new ChangeStatusOfVacationRequestRequest { Id = id, CurrentEmployeeId = user.GetId()! });

        return Results.Ok();
    }

    private static async Task<IResult> RejectVacationRequestAsync(
        VacationRequests.VacationRequestsClient vacationRequestsClient,
        [FromRoute] int id,
        ClaimsPrincipal user)
    {
        await vacationRequestsClient.RejectAsync(new ChangeStatusOfVacationRequestRequest { Id = id, CurrentEmployeeId = user.GetId()! });

        return Results.Ok();
    }
}
