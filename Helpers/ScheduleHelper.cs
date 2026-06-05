using WebApi.DTO;
using WebApi.Services;

public class ScheduleHelper
{
    public int GetWorkingDaysInMonth(int year, int month, IOrgService orgService)
    {
        int workingDays = 0;
        int daysInMonth = DateTime.DaysInMonth(year, month);
        var holidayList = orgService.GetOrgHolidays().GetAwaiter().GetResult();

        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime currentDay = new(year, month, day);
            bool isHoliday = holidayList.Any(nonWorkingDay =>
                currentDay.Date == nonWorkingDay.Date ||
                (nonWorkingDay.Type.Equals("Weekend", StringComparison.OrdinalIgnoreCase) &&
                 currentDay.DayOfWeek.ToString().Equals(nonWorkingDay.Name, StringComparison.OrdinalIgnoreCase)));

            if (!isHoliday) workingDays++;
        }

        return workingDays;
    }

    public List<(int Year, int Month)> GetMonthsBetween(DateOnly startDate, DateOnly endDate)
    {
        var months = new List<(int Year, int Month)>();
        var current = new DateTime(startDate.Year, startDate.Month, 1);
        var end = new DateTime(endDate.Year, endDate.Month, 1);

        while (current <= end)
        {
            months.Add((current.Year, current.Month));
            current = current.AddMonths(1);
        }

        return months;
    }

    public List<Schedule> GetWorkingDaysForDuration(DateOnly startDate, DateOnly endDate, IOrgService orgService)
    {
        var schedules = new List<Schedule>();
        var monthList = GetMonthsBetween(startDate, endDate);

        foreach (var (year, month) in monthList)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            //string label = $"1/{month} - {daysInMonth}/{month}";
            string label = new DateTime(year, month, 1).ToString("MMM yyyy");

            int workingDays = GetWorkingDaysInMonth(year, month, orgService);
            schedules.Add(new Schedule { Year = year, MonthNo = month, Month = label, WorkingDays = workingDays, WorkingHours = workingDays * 8});
        }

        return schedules;
    }


    public List<Schedule> GetWorkingDaysForDuration(DateOnly startDate, DateOnly endDate)
    {
        var schedules = new List<Schedule>();
        var monthList = GetMonthsBetween(startDate, endDate);

        foreach (var (year, month) in monthList)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);

            string label = new DateTime(year, month, 1)
                .ToString("MMM yyyy");

            int workingDays = GetWorkingDaysInMonth(year, month);

            schedules.Add(new Schedule
            {
                Year = year,
                MonthNo = month,
                Month = label,
                WorkingDays = workingDays,
                WorkingHours = workingDays * 8
            });
        }

        return schedules;
    }
    private int GetWorkingDaysInMonth(int year, int month)
    {
        int workingDays = 0;
        int daysInMonth = DateTime.DaysInMonth(year, month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);

            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
            {
                workingDays++;
            }
        }

        return workingDays;
    }
}
