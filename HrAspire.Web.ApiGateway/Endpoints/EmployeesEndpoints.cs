namespace HrAspire.Web.ApiGateway.Endpoints;

using HrAspire.Employees.Web;
using HrAspire.Web.ApiGateway.Mappers;

using Microsoft.AspNetCore.Mvc;

public static class EmployeesEndpoints
{
    public static IEndpointConventionBuilder MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var employeesGroup = endpoints.MapGroup("/Employees");

        employeesGroup
            .MapGet(
                "/Page",
                async (Employees.EmployeesClient employeesClient, [FromQuery] int pageNumber = 0, [FromQuery] int pageSize = 10) =>
                {
                    var employeesResponse = await employeesClient.GetEmployeesPageAsync(
                        new GetEmployeesPageRequest { PageNumber = pageNumber, PageSize = pageSize });

                    var employees = employeesResponse.Employees.Select(e => e.MapToResponseModel()).ToList();

                    return employees;
                });
        // .RequireAuthorization(); // TODO:

        return employeesGroup;
    }
}
