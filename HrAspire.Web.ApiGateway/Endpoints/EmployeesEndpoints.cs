namespace HrAspire.Web.ApiGateway.Endpoints;

using HrAspire.Employees.Web;

using Microsoft.AspNetCore.Mvc;

public static class EmployeesEndpoints
{
    public static IEndpointConventionBuilder MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var employeesGroup = endpoints.MapGroup("/Employees");

        employeesGroup
            .MapGet(
                "/Page",
                async ([FromQuery] int pageNumber, [FromQuery] int pageSize, Employees.EmployeesClient employeesClient) =>
                {
                    var employeesResponse = await employeesClient.GetEmployeesPageAsync(
                        new GetEmployeesPageRequest { PageNumber = pageNumber, PageSize = pageSize });

                    return employeesResponse.Employees;
                });
            // .RequireAuthorization(); // TODO:

        return employeesGroup;
    }
}
