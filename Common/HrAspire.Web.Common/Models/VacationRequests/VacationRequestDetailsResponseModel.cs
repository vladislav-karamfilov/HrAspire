namespace HrAspire.Web.Common.Models.VacationRequests;

using HrAspire.Vacations.Data.Models;

public record VacationRequestDetailsResponseModel(
    int Id,
    string EmployeeId,
    string EmployeeFullName,
    VacationRequestType Type,
    DateOnly FromDate,
    DateOnly ToDate,
    int WorkDays,
    string? Notes,
    VacationRequestStatus Status,
    DateTime? StatusChangedOn,
    string? StatusChangedById,
    string? StatusChangedByFullName,
    DateTime CreatedOn);
