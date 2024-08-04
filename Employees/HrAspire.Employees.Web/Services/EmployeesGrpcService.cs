namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Web.Mappers;
using HrAspire.Web.Common;

public class EmployeesGrpcService : Employees.EmployeesBase
{
    private readonly IEmployeesService employeesService;

    public EmployeesGrpcService(IEmployeesService employeesService)
    {
        this.employeesService = employeesService;
    }

    public override async Task<GetEmployeesResponse> GetEmployees(GetEmployeesRequest request, ServerCallContext context)
    {
        var employees = await this.employeesService.GetEmployeesAsync(request.CurrentEmployeeId, request.PageNumber, request.PageSize);
        var total = await this.employeesService.GetEmployeesCountAsync(request.CurrentEmployeeId);

        var response = new GetEmployeesResponse { Total = total };
        foreach (var employee in employees)
        {
            response.Employees.Add(employee.MapToEmployeeModel());
        }

        return response;
    }

    public override async Task<GetEmployeeResponse> GetEmployee(GetEmployeeRequest request, ServerCallContext context)
    {
        var employee = await this.employeesService.GetEmployeeAsync(request.Id);
        if (employee is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        var response = new GetEmployeeResponse { Employee = employee.MapToEmployeeDetails() };
        return response;
    }

    public override async Task<CreateEmployeeResponse> CreateEmployee(CreateEmployeeRequest request, ServerCallContext context)
    {
        var createResult = await this.employeesService.CreateAsync(
            request.Email,
            request.Password,
            request.FullName,
            request.DateOfBirth.ToDateOnly(),
            request.Position,
            request.Department,
            request.ManagerId,
            request.Role,
            request.CreatedById);

        if (createResult.IsError)
        {
            throw createResult.ToRpcException();
        }

        return new CreateEmployeeResponse { Id = createResult.Data };
    }

    public override async Task<Empty> UpdateEmployee(UpdateEmployeeRequest request, ServerCallContext context)
    {
        var updateResult = await this.employeesService.UpdateAsync(
            request.Id,
            request.FullName,
            request.DateOfBirth.ToDateOnly(),
            request.Position,
            request.Department,
            request.Role,
            request.ManagerId);

        if (updateResult.IsError)
        {
            throw updateResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> DeleteEmployee(DeleteEmployeeRequest request, ServerCallContext context)
    {
        var deleteResult = await this.employeesService.DeleteAsync(request.Id);
        if (deleteResult.IsError)
        {
            throw deleteResult.ToRpcException();
        }

        return new Empty();
    }
}
