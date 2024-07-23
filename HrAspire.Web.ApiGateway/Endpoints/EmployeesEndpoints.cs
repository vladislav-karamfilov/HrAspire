namespace HrAspire.Web.ApiGateway.Endpoints;

using System.Net;
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
        var employeesGroup = endpoints.MapGroup("/Employees").RequireAuthorization();

        employeesGroup.MapGet(
            "/",
            async (Employees.EmployeesClient employeesClient, [FromQuery] int pageNumber = 0, [FromQuery] int pageSize = 10) =>
            {
                var employeesResponse = await employeesClient.GetEmployeesPageAsync(
                    new GetEmployeesPageRequest { PageNumber = pageNumber, PageSize = pageSize });

                var employees = employeesResponse.Employees.Select(e => e.MapToResponseModel()).ToList();

                return new EmployeesPageResponseModel(employees, employeesResponse.Total);
            });

        employeesGroup.MapPost(
            "/",
            async (Employees.EmployeesClient employeesClient, [FromBody] EmployeeCreateRequestModel model, ClaimsPrincipal user) =>
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

                if (string.IsNullOrWhiteSpace(createResponse.ErrorMessage))
                {
                    return Results.Created(string.Empty, createResponse.Id);
                }

                return Results.Problem(createResponse.ErrorMessage, statusCode: (int)HttpStatusCode.BadRequest);
            });

        employeesGroup.MapGet(
            "/{id}",
            async (Employees.EmployeesClient employeesClient, [FromRoute] string id) =>
            {
                var employeeResponse = await employeesClient.GetEmployeeAsync(new GetEmployeeRequest { Id = id });
                if (employeeResponse.Employee is null)
                {
                    return null;
                }

                var employee = employeeResponse.Employee.MapToDetailsResponseModel();

                return employee;
            });

        employeesGroup.MapPut(
            "/{id}",
            async (Employees.EmployeesClient employeesClient, [FromRoute] string id, [FromBody] EmployeeUpdateRequestModel model) =>
            {
                // TODO: Make sure the model cannot come unvalidated!!!
                var updateResponse = await employeesClient.UpdateEmployeeAsync(new UpdateEmployeeRequest
                {
                    Id = id,
                    FullName = model.FullName,
                    Position = model.Position,
                    DateOfBirth = model.DateOfBirth.ToTimestamp(),
                    Department = model.Department,
                    ManagerId = model.ManagerId,
                });

                if (string.IsNullOrWhiteSpace(updateResponse.ErrorMessage))
                {
                    return Results.Ok();
                }

                return Results.Problem(updateResponse.ErrorMessage, statusCode: (int)HttpStatusCode.BadRequest);
            });

        employeesGroup.MapDelete(
            "/{id}",
            async (Employees.EmployeesClient employeesClient, [FromRoute] string id) =>
            {
                var deleteResponse = await employeesClient.DeleteEmployeeAsync(new DeleteEmployeeRequest { Id = id });
                if (string.IsNullOrWhiteSpace(deleteResponse.ErrorMessage))
                {
                    return Results.Ok();
                }

                return Results.Problem(deleteResponse.ErrorMessage, statusCode: (int)HttpStatusCode.BadRequest);
            });

        return employeesGroup;
    }
}
