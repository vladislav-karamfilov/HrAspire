namespace HrAspire.Business.Common;

public static class PaginationHelper
{
    public static void Normalize(ref int pageNumber, ref int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 0);

        if (pageSize <= 0)
        {
            pageSize = 10;
        }
        else if (pageSize > 100)
        {
            pageSize = 100;
        }
    }
}
