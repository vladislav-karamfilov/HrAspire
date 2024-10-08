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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1116:Split parameters should start on line after declaration",
        Justification = "Better readability.")]
    public static IEndpointConventionBuilder MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/Employees").RequireAuthorization();

        group
            .MapGet(
                "/",
                (Employees.EmployeesClient employeesClient,
                    ClaimsPrincipal user,
                    [FromQuery] int pageNumber = 0,
                    [FromQuery] int pageSize = 10)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        var employeesResponse = await employeesClient.ListAsync(
                            new ListEmployeesRequest
                            {
                                CurrentEmployeeId = user.GetId()!,
                                PageNumber = pageNumber,
                                PageSize = pageSize,
                            });

                        var employees = employeesResponse.Employees.Select(e => e.MapToResponseModel()).ToList();

                        return Results.Ok(new EmployeesResponseModel(employees, employeesResponse.Total));
                    }))
            .RequireAuthorization(Constants.ManagerOrHrManagerAuthPolicyName);

        group
            .MapGet(
                "/Managers",
                (Employees.EmployeesClient employeesClient)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        var managersResponse = await employeesClient.ListManagersAsync(new Empty());

                        var managers = managersResponse.Employees.Select(e => e.MapToResponseModel()).ToList();

                        return Results.Ok(managers);
                    }))
            .RequireAuthorization(Constants.HrManagerAuthPolicyName);

        group
            .MapPost(
                "/",
                (Employees.EmployeesClient employeesClient, [FromBody] EmployeeCreateRequestModel model, ClaimsPrincipal user)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        var createResponse = await employeesClient.CreateAsync(new CreateEmployeeRequest
                        {
                            Email = model.Email,
                            FullName = model.FullName,
                            Password = model.Password,
                            Position = model.Position,
                            DateOfBirth = model.DateOfBirth!.Value.ToTimestamp(),
                            Department = model.Department,
                            ManagerId = string.IsNullOrWhiteSpace(model.ManagerId) ? null : model.ManagerId,
                            Salary = model.Salary,
                            Role = string.IsNullOrWhiteSpace(model.Role) ? null : model.Role,
                            CreatedById = user.GetId()!,
                        });

                        return Results.Created(string.Empty, createResponse.Id);
                    }))
            .RequireAuthorization(Constants.HrManagerAuthPolicyName);

        group.MapGet(
            "/{id}",
            (Employees.EmployeesClient employeesClient, [FromRoute] string id, ClaimsPrincipal user)
                => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                {
                    var employeeResponse = await employeesClient.GetAsync(
                        new GetEmployeeRequest { Id = id, CurrentEmployeeId = user.GetId()! });

                    var employee = employeeResponse.Employee.MapToDetailsResponseModel();

                    return Results.Ok(employee);
                }));

        group
            .MapPut(
                "/{id}",
                (Employees.EmployeesClient employeesClient, [FromRoute] string id, [FromBody] EmployeeUpdateRequestModel model)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
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
                    }))
            .RequireAuthorization(Constants.HrManagerAuthPolicyName);

        group
            .MapDelete(
                "/{id}",
                (Employees.EmployeesClient employeesClient, [FromRoute] string id)
                    => GrpcToHttpHelper.HandleGrpcCallAsync(async () =>
                    {
                        await employeesClient.DeleteAsync(new DeleteEmployeeRequest { Id = id });

                        return Results.Ok();
                    }))
            .RequireAuthorization(Constants.HrManagerAuthPolicyName);

        return group;
    }
}
