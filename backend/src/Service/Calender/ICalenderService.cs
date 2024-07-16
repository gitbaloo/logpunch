using Shared;
using System;
using System.Collections;
using System.Threading.Tasks;
using Domain;

namespace Infrastructure
{
    public interface ICalenderService
    {
        Task<int> HolidaysAndWeekendDatesInTimeSpan(DateTime startDate, DateTime endDate);
        Task<ICollection<Holiday>?> GetHolidays(DateTime startDate, DateTime endDate);
        Task<ICollection<Holiday>?> GetOnlyNationalHolidays(DateTime startDate, DateTime endDate);
        Task<bool> IsDateValid(DateTimeOffset date);
    }
}
