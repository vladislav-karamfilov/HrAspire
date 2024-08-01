namespace HrAspire.Web.Client;

public static class Constants
{
    public const string UnexpectedErrorMessage = "An unexpected error has occurred. Please try again later.";

    public const string DateFormat = "dd-MM-yyyy";
    public const string DateTimeFormat = "dd-MM-yyyy HH:mm:ss";

    public const int DocumentMaxFileSizeInMB = 3;
    public const int DocumentMaxFileSizeInB = DocumentMaxFileSizeInMB * 1024 * 1024;
}
