namespace HrAspire.Web.Common.Models.VacationRequests;

using System.ComponentModel.DataAnnotations;

using HrAspire.Vacations.Data.Models;

public class VacationRequestCreateRequestModel
{
    [Required]
    public VacationRequestType? Type { get; set; }

    // TODO: validate from date < to date + from date >= today
    [Required]
    public DateOnly? FromDate { get; set; }

    [Required]
    public DateOnly? ToDate { get; set; }

    public string? Notes { get; set; }
}
