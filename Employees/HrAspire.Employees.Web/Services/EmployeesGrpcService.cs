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

    public override async Task<ListEmployeesResponse> List(ListEmployeesRequest request, ServerCallContext context)
    {
        var employees = await this.employeesService.ListAsync(request.CurrentEmployeeId, request.PageNumber, request.PageSize);
        var total = await this.employeesService.GetCountAsync(request.CurrentEmployeeId);

        var response = new ListEmployeesResponse { Total = total };
        foreach (var employee in employees)
        {
            response.Employees.Add(employee.MapToEmployeeGrpcModel());
        }

        return response;
    }

    public override async Task<ListEmployeesResponse> ListManagers(Empty request, ServerCallContext context)
    {
        var managers = await this.employeesService.ListManagersAsync();

        var response = new ListEmployeesResponse();
        foreach (var manager in managers)
        {
            response.Employees.Add(manager.MapToEmployeeGrpcModel());
            response.Total++;
        }

        return response;
    }

    public override async Task<ListEmployeesResponse> ListManagedEmployees(ListManagedEmployeesRequest request, ServerCallContext context)
    {
        var managedEmployees = await this.employeesService.ListManagedEmployeesAsync(request.ManagerId);

        var response = new ListEmployeesResponse();
        foreach (var manager in managedEmployees)
        {
            response.Employees.Add(manager.MapToEmployeeGrpcModel());
            response.Total++;
        }

        return response;
    }

    public override async Task<GetEmployeeResponse> Get(GetEmployeeRequest request, ServerCallContext context)
    {
        var employee = await this.employeesService.GetAsync(request.Id);
        if (employee is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, detail: string.Empty));
        }

        var response = new GetEmployeeResponse { Employee = employee.MapToEmployeeDetailsGrpcModel() };
        return response;
    }

    public override async Task<CreateEmployeeResponse> Create(CreateEmployeeRequest request, ServerCallContext context)
    {
        var createResult = await this.employeesService.CreateAsync(
            request.Email,
            request.Password,
            request.FullName,
            request.DateOfBirth.ToDateOnly(),
            request.Position,
            request.Department,
            request.ManagerId,
            request.Salary,
            request.Role,
            request.CreatedById);

        if (createResult.IsError)
        {
            throw createResult.ToRpcException();
        }

        return new CreateEmployeeResponse { Id = createResult.Data };
    }

    public override async Task<Empty> Update(UpdateEmployeeRequest request, ServerCallContext context)
    {
        var updateResult = await this.employeesService.UpdateAsync(
            request.Id,
            request.FullName,
            request.DateOfBirth.ToDateOnly(),
            request.Position,
            request.Department,
            request.ManagerId,
            request.Role);

        if (updateResult.IsError)
        {
            throw updateResult.ToRpcException();
        }

        return new Empty();
    }

    public override async Task<Empty> Delete(DeleteEmployeeRequest request, ServerCallContext context)
    {
        var deleteResult = await this.employeesService.DeleteAsync(request.Id);
        if (deleteResult.IsError)
        {
            throw deleteResult.ToRpcException();
        }

        return new Empty();
    }
}
