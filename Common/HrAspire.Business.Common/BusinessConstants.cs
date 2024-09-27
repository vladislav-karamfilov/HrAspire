namespace HrAspire.Business.Common;

public static class BusinessConstants
{
    public const string ManagerRole = "Manager";
    public const string HrManagerRole = "HrManager";
    public const string ManagerAndHrManagerRoles = ManagerRole + "," + HrManagerRole;

    public const string EmployeeNamesCacheSetName = "employee-names";

    public const int MaxPaidVacationDaysPerYear = 20;
}
