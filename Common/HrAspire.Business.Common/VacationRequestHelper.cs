namespace HrAspire.Business.Common;

public static class VacationRequestHelper
{
    public static int CalculateWorkDaysBetweenDates(DateOnly fromDate, DateOnly toDate)
    {
        if (fromDate > toDate)
        {
            throw new ArgumentException("From date cannot be after To date.", nameof(fromDate));
        }

        var workDays = 0;
        var current = fromDate;
        while (current <= toDate)
        {
            if (current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                workDays++;
            }

            current = current.AddDays(1);
        }

        return workDays;
    }
}
