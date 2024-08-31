namespace HrAspire.Web.ApiGateway.Endpoints;

using HrAspire.Salaries.Web;
using HrAspire.Web.ApiGateway.Mappers;
using HrAspire.Web.Common.Models.SalaryRequests;

using Microsoft.AspNetCore.Mvc;

public static class SalaryRequestsEndpoints
{
    public static IEndpointConventionBuilder MapSalaryRequestsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/SalaryRequests").RequireAuthorization();

        group
            .MapGet(
                "/",
                (SalaryRequests.SalaryRequestsClient salaryRequestsClient,
                    [FromQuery] int pageNumber = 0,
                    [FromQuery] int pageSize = 10)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        var salaryRequestsResponse = await salaryRequestsClient.GetSalaryRequestsAsync(
                            new GetSalaryRequestsRequest { PageNumber = pageNumber, PageSize = pageSize });

                        var salaryRequests = salaryRequestsResponse.SalaryRequests.Select(e => e.MapToResponseModel()).ToList();

                        return Results.Ok(new SalaryRequestsResponseModel(salaryRequests, salaryRequestsResponse.Total));
                    }))
            .RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        return group;
    }
}
