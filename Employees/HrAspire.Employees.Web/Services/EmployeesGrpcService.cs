namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using Grpc.Core;

using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Web.Mappers;
using HrAspire.Web.Common.Extensions;

public class EmployeesGrpcService : Employees.EmployeesBase
{
    private readonly IEmployeesService employeesService;

    public EmployeesGrpcService(IEmployeesService employeesService)
    {
        this.employeesService = employeesService;
    }

    public override async Task<GetEmployeesPageResponse> GetEmployeesPage(GetEmployeesPageRequest request, ServerCallContext context)
    {
        var employees = await this.employeesService.GetEmployeesPageAsync(request.PageNumber, request.PageSize);
        var total = await this.employeesService.GetEmployeesCountAsync();

        var response = new GetEmployeesPageResponse { Total = total };
        foreach (var employee in employees)
        {
            response.Employees.Add(employee.MapToPageEmployee());
        }

        return response;
    }

    public override async Task<GetEmployeeResponse> GetEmployee(GetEmployeeRequest request, ServerCallContext context)
    {
        var employee = await this.employeesService.GetEmployeeAsync(request.Id);

        var response = new GetEmployeeResponse { Employee = employee?.MapToEmployeeDetails() };

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
            request.CreatedById);

        return new CreateEmployeeResponse { Id = createResult.Data, ErrorMessage = createResult.ErrorMessage };
    }

    public override async Task<UpdateEmployeeResponse> UpdateEmployee(UpdateEmployeeRequest request, ServerCallContext context)
    {
        var updateResult = await this.employeesService.UpdateAsync(
            request.Id,
            request.FullName,
            request.DateOfBirth.ToDateOnly(),
            request.Position,
            request.Department,
            request.ManagerId);

        return new UpdateEmployeeResponse { ErrorMessage = updateResult.ErrorMessage };
    }

    public override async Task<DeleteEmployeeResponse> DeleteEmployee(DeleteEmployeeRequest request, ServerCallContext context)
    {
        var deleteResult = await this.employeesService.DeleteAsync(request.Id);

        return new DeleteEmployeeResponse { ErrorMessage = deleteResult.ErrorMessage };
    }
}
