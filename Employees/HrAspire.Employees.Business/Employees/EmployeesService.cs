namespace HrAspire.Employees.Business.Employees;

using System.Collections.Generic;
using System.Threading.Tasks;

using HrAspire.Employees.Data;

using Microsoft.EntityFrameworkCore;

public class EmployeesService : IEmployeesService
{
    private readonly EmployeesDbContext dbContext;

    public EmployeesService(EmployeesDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<EmployeeDetailsServiceModel?> GetEmployeeAsync(string id)
        => this.dbContext.Employees.Where(e => e.Id == id).ProjectToDetailsServiceModel().FirstOrDefaultAsync();

    public async Task<IEnumerable<EmployeeServiceModel>> GetEmployeesPageAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 0)
        {
            pageNumber = 0;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }
        else if (pageSize > 100)
        {
            pageSize = 100;
        }

        return await this.dbContext.Employees
            .OrderByDescending(e => e.CreatedOn)
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ProjectToServiceModel()
            .ToListAsync();
    }
}
