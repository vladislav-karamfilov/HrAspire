namespace HrAspire.Web.Common.Models.SalaryRequests;

using System.ComponentModel.DataAnnotations;

public class SalaryRequestCreateRequestModel
{
    [Range(0, int.MaxValue)]
    public decimal NewSalary { get; set; }

    public string? Notes { get; set; }
}
