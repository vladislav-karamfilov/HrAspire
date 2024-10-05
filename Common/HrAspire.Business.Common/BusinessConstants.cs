namespace HrAspire.Business.Common;

public static class BusinessConstants
{
    public const string ManagerRole = "Manager";
    public const string HrManagerRole = "HrManager";
    public const string ManagerAndHrManagerRoles = ManagerRole + "," + HrManagerRole;

    public const string EmployeesCacheSetName = "employees";

    public const int MaxPaidVacationDaysPerYear = 20;

    public static readonly HashSet<string> AllowedDocumentFileExtensions = new(
        [
            ".txt", ".doc", ".docm", ".docx", ".log", ".odt", ".rtf", ".json",
            ".pdf", ".ppt", ".pptm", ".pptx", ".xls", ".xlsm", ".xlsx",
            ".7z", ".zip", ".rar", ".gz", ".tar",
            ".png", ".jpg", ".jpeg", ".webp", ".gif", ".tiff", ".pbm", ".bmp", ".tga",
        ],
        StringComparer.OrdinalIgnoreCase);
}
