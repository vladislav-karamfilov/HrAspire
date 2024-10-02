namespace HrAspire.Web.Common.Models.VacationRequests;

using HrAspire.Vacations.Data.Models;

public class VacationRequestCreateRequestModel
{
    public VacationRequestType? Type { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public string? Notes { get; set; }
}
