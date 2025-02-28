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

        group.MapGet("/", GetEmployeesAsync).RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        group.MapGet("/Managers", GetManagersAsync).RequireAuthorization(Constants.HrManagerAuthPolicyName);

        group.MapGet("/{id}", GetEmployeeAsync);

        group.MapPost("/", CreateEmployeeAsync).RequireAuthorization(Constants.HrManagerAuthPolicyName);

        group.MapPut("/{id}", UpdateEmployeeAsync).RequireAuthorization(Constants.HrManagerAuthPolicyName);

        group.MapDelete("/{id}", DeleteEmployeeAsync).RequireAuthorization(Constants.HrManagerAuthPolicyName);

        return group;
    }

    private static async Task<IResult> GetEmployeesAsync(
        Employees.EmployeesClient employeesClient,
        ClaimsPrincipal user,
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10)
    {
        var employeesResponse = await employeesClient.ListAsync(new ListEmployeesRequest
        {
            CurrentEmployeeId = user.GetId()!,
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        var employees = employeesResponse.Employees.Select(e => e.MapToResponseModel()).ToList();

        return Results.Ok(new EmployeesResponseModel(employees, employeesResponse.Total));
    }

    private static async Task<IResult> GetManagersAsync(Employees.EmployeesClient employeesClient)
    {
        var managersResponse = await employeesClient.ListManagersAsync(new Empty());

        var managers = managersResponse.Employees.Select(e => e.MapToResponseModel()).ToList();

        return Results.Ok(managers);
    }

    private static async Task<IResult> GetEmployeeAsync(
        Employees.EmployeesClient employeesClient,
        [FromRoute] string id,
        ClaimsPrincipal user)
    {
        var employeeResponse = await employeesClient.GetAsync(new GetEmployeeRequest { Id = id, CurrentEmployeeId = user.GetId()! });

        var employee = employeeResponse.Employee.MapToDetailsResponseModel();

        return Results.Ok(employee);
    }

    private static async Task<IResult> CreateEmployeeAsync(
        Employees.EmployeesClient employeesClient,
        [FromBody] EmployeeCreateRequestModel model,
        ClaimsPrincipal user)
    {
        var createResponse = await employeesClient.CreateAsync(new CreateEmployeeRequest
        {
            Email = model.Email,
            FullName = model.FullName,
            Password = model.Password,
            Position = model.Position,
            DateOfBirth = model.DateOfBirth.GetValueOrDefault().ToTimestamp(),
            Department = model.Department,
            ManagerId = string.IsNullOrWhiteSpace(model.ManagerId) ? null : model.ManagerId,
            Salary = model.Salary,
            Role = string.IsNullOrWhiteSpace(model.Role) ? null : model.Role,
            CreatedById = user.GetId()!,
        });

        return Results.Created(string.Empty, createResponse.Id);
    }

    private static async Task<IResult> UpdateEmployeeAsync(
        Employees.EmployeesClient employeesClient,
        [FromRoute] string id,
        [FromBody] EmployeeUpdateRequestModel model)
    {
        await employeesClient.UpdateAsync(new UpdateEmployeeRequest
        {
            Id = id,
            FullName = model.FullName,
            Position = model.Position,
            DateOfBirth = model.DateOfBirth.ToTimestamp(),
            Department = model.Department,
            ManagerId = string.IsNullOrWhiteSpace(model.ManagerId) ? null : model.ManagerId,
            Role = string.IsNullOrWhiteSpace(model.Role) ? null : model.Role,
        });

        return Results.Ok();
    }

    private static async Task<IResult> DeleteEmployeeAsync(Employees.EmployeesClient employeesClient, [FromRoute] string id)
    {
        await employeesClient.DeleteAsync(new DeleteEmployeeRequest { Id = id });

        return Results.Ok();
    }
}
