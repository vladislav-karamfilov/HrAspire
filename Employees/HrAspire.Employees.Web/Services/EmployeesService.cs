namespace HrAspire.Employees.Web.Services;

using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using HrAspire.Employees.Business.Employees;

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
            response.Employees.Add(new PageEmployee
            {
                FullName = employee.FullName,
                DateOfBirth = employee.DateOfBirth.ToDateTime(default, DateTimeKind.Utc).ToTimestamp(),
                CreatedOn = employee.CreatedOn.ToTimestamp(),
                Department = employee.Department,
                Position = employee.Position,
            });
        }

        return response;
    }
}
