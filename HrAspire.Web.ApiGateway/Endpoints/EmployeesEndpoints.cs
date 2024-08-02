namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Security.Claims;

using Google.Protobuf.WellKnownTypes;

using HrAspire.Employees.Web;
using HrAspire.Web.ApiGateway.Mappers;
using HrAspire.Web.Common;
using HrAspire.Web.Common.Models.Employees;

using Microsoft.AspNetCore.Mvc;

public static class EmployeesEndpoints
{
    public static IEndpointConventionBuilder MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Employees").RequireAuthorization();

        group.MapGet(
            "/",
            (Employees.EmployeesClient employeesClient, [FromQuery] int pageNumber = 0, [FromQuery] int pageSize = 10)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var employeesResponse = await employeesClient.GetEmployeesAsync(
                        new GetEmployeesRequest { PageNumber = pageNumber, PageSize = pageSize });

                    var employees = employeesResponse.Employees.Select(e => e.MapToResponseModel()).ToList();

                    return Results.Ok(new EmployeesResponseModel(employees, employeesResponse.Total));
                }));

        group.MapPost(
            "/",
            (Employees.EmployeesClient employeesClient, [FromBody] EmployeeCreateRequestModel model, ClaimsPrincipal user)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    // TODO: Make sure the model cannot come unvalidated!!!
                    var createResponse = await employeesClient.CreateEmployeeAsync(new CreateEmployeeRequest
                    {
                        Email = model.Email,
                        FullName = model.FullName,
                        Password = model.Password,
                        Position = model.Position,
                        DateOfBirth = model.DateOfBirth!.Value.ToTimestamp(),
                        Department = model.Department,
                        ManagerId = model.ManagerId,
                        CreatedById = user.GetId()!,
                    });

                    return Results.Created(string.Empty, createResponse.Id);
                }));

        group.MapGet(
            "/{id}",
            (Employees.EmployeesClient employeesClient, [FromRoute] string id)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var employeeResponse = await employeesClient.GetEmployeeAsync(new GetEmployeeRequest { Id = id });

                    var employee = employeeResponse.Employee.MapToDetailsResponseModel();

                    return Results.Ok(employee);
                }));

        group.MapPut(
            "/{id}",
            (Employees.EmployeesClient employeesClient, [FromRoute] string id, [FromBody] EmployeeUpdateRequestModel model)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    // TODO: Make sure the model cannot come unvalidated!!!
                    await employeesClient.UpdateEmployeeAsync(new UpdateEmployeeRequest
                    {
                        Id = id,
                        FullName = model.FullName,
                        Position = model.Position,
                        DateOfBirth = model.DateOfBirth.ToTimestamp(),
                        Department = model.Department,
                        ManagerId = model.ManagerId,
                    });

                    return Results.Ok();
                }));

        group.MapDelete(
            "/{id}",
            (Employees.EmployeesClient employeesClient, [FromRoute] string id)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    await employeesClient.DeleteEmployeeAsync(new DeleteEmployeeRequest { Id = id });

                    return Results.Ok();
                }));

        return group;
    }
}
