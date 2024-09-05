namespace HrAspire.Business.Common.Events;

public record SalaryRequestApprovedEvent(string EmployeeId, decimal NewSalary, DateTime ApprovedOn);
