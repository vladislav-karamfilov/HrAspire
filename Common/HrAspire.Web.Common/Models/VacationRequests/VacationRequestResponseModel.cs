namespace HrAspire.Web.Common.Models.VacationRequests;

using HrAspire.Vacations.Data.Models;

public record VacationRequestResponseModel(
    int Id,
    string EmployeeId,
    string EmployeeFullName,
    VacationRequestType Type,
    DateOnly FromDate,
    DateOnly ToDate,
    VacationRequestStatus Status,
    DateTime CreatedOn);
