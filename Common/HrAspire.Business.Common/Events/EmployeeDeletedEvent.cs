namespace HrAspire.Business.Common.Events;

public record EmployeeDeletedEvent(string EmployeeId, DateTime DeletedOn);
