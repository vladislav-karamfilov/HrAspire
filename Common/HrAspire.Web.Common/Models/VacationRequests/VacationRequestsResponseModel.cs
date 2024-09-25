namespace HrAspire.Web.Common.Models.VacationRequests;

public record VacationRequestsResponseModel(ICollection<VacationRequestResponseModel> VacationRequests, int Total);
