namespace HrAspire.Business.Common.Events;

public record PaidVacationRequestApprovedEvent(
    string EmployeeId,
    DateOnly FromDate,
    DateOnly ToDate,
    int TotalUsedPaidVacationDays,
    DateTime ApprovedOn);
