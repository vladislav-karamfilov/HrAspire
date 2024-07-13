namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using Grpc.Core;

using HrAspire.Employees.Business.Employees;
using HrAspire.Employees.Web.Mappers;

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

        var response = new GetEmployeesPageResponse();
        foreach (var employee in employees)
        {
            response.Employees.Add(employee.MapToPageEmployee());
        }

        return response;
    }

    public override async Task<GetEmployeeResponse> GetEmployee(GetEmployeeRequest request, ServerCallContext context)
    {
        var employee = await this.employeesService.GetEmployeeAsync(request.Id);

        var response = new GetEmployeeResponse { Employee = employee?.MapToEmployeeDetails(), };

        return response;
    }
}
