namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Security.Claims;

using HrAspire.Salaries.Web;
using HrAspire.Web.ApiGateway.Mappers;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.SalaryRequests;

using Microsoft.AspNetCore.Mvc;

public static class SalaryRequestsEndpoints
{
    public static IEndpointConventionBuilder MapSalaryRequestsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/").RequireAuthorization();

        // TODO: add managerId param to GRPC method (like employees endpoint)
        group
            .MapGet(
                "/SalaryRequests",
                (SalaryRequests.SalaryRequestsClient salaryRequestsClient,
                    [FromQuery] int pageNumber = 0,
                    [FromQuery] int pageSize = 10)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        var salaryRequestsResponse = await salaryRequestsClient.ListAsync(
                            new GetSalaryRequestsRequest { PageNumber = pageNumber, PageSize = pageSize });

                        var salaryRequests = salaryRequestsResponse.SalaryRequests.Select(e => e.MapToResponseModel()).ToList();

                        return Results.Ok(new SalaryRequestsResponseModel(salaryRequests, salaryRequestsResponse.Total));
                    }))
            .RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        group
            .MapPost(
                "/Employees/{employeeId}/SalaryRequests",
                (SalaryRequests.SalaryRequestsClient salaryRequestsClient,
                    [FromRoute] string employeeId,
                    [FromBody] SalaryRequestCreateRequestModel model,
                    ClaimsPrincipal user)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        // TODO: Make sure the model cannot come unvalidated!!!
                        var createResponse = await salaryRequestsClient.CreateAsync(new CreateSalaryRequestRequest
                        {
                            EmployeeId = employeeId,
                            NewSalary = model.NewSalary,
                            Notes = model.Notes,
                            CreatedById = user.GetId()!,
                        });

                        return Results.Created(string.Empty, createResponse.Id);
                    }));
        // TODO: .RequireAuthorization(Constants.ManagerAuthPolicyName);

        return group;
    }
}
