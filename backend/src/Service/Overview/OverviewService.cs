using System.Formats.Asn1;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Persistence;
using Service.Login;
using Shared;

namespace Infrastructure.Overview;

public class OverviewService(LogpunchDbContext dbContext, ILoginService loginService) : IOverviewService
{
    public async Task<OverviewResponse> OverviewQuery(string token, bool sortAsc, bool showDaysWithNoRecords,
        bool setDefault, DateTime startDate, DateTime? endDate, string timePeriod, string timeMode, string groupBy,
        string thenBy)
    {
        var user = await loginService.ValidateToken(token);
        DateTime originalStartDate = startDate;
        string queryString;

        if (endDate is null)
        {
            queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, set_default={setDefault}, start_date=null, end_date={endDate}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
        }
        else
        {
            queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, set_default={setDefault}, start_date={startDate.ToShortDateString()}, end_date={endDate.Value.ToShortDateString()}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
        }

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (timeMode != "custom" && endDate.HasValue || timePeriod != "custom" && endDate.HasValue)
        {
            throw new ArgumentException("Invalid input combination: timeMode and timePeriod have to be 'custom' if you choose an endDate or endDate has to be left blank to see the chosen timeMode/timePeriod combination.");
        }

        if (showDaysWithNoRecords && groupBy != "day" && thenBy != "day")
        {
            throw new ArgumentException($"Invalid input combination: groupBy or thenBy have to be 'day' in order to show days with no records. showDaysWithNoRecords was {showDaysWithNoRecords} while groupBy was {groupBy} and thenBy was {thenBy}.");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("Invalid endDate: endDate can't be earlier than startDate");
        }

        if (timePeriod != "custom" && endDate is null)
        {
            startDate = FindStartDateOfTimePeriod(startDate, timePeriod, timeMode);
            endDate = FindEndDate(originalStartDate, startDate, timePeriod, timeMode);
        }


        DateTime utcStartDate = startDate.ToUniversalTime().AddHours(1);

        DateTime utcEndDate = default;

        // DEMO FIX: REMOVE AFTER DEMO
        if (endDate.HasValue)
        {
            utcEndDate = endDate.Value.ToUniversalTime();
            utcEndDate = utcEndDate.Add(new TimeSpan(24, 59, 59));
        }

        Console.WriteLine($"Start date: {utcStartDate} End date: {utcEndDate}");

        if (!IsGroupByValid(groupBy, utcStartDate, utcEndDate))
        {
            throw new ArgumentException($"Invalid groupby option selected: A time period can only be grouped into period units that it contains more than one of. startDate: {utcStartDate} endDate: {utcEndDate}");
        }

        var groupByObjects = await GetGroupByObjects(groupBy, user, utcStartDate, utcEndDate, showDaysWithNoRecords);
        double total = 0;

        foreach (var groupByObject in groupByObjects)
        {
            total += groupByObject.Total;
        }

        if (!sortAsc)
        {
            foreach (GroupByObject groupByObject in groupByObjects)
            {
                groupByObject.ThenByObjects.Reverse();
            }

            groupByObjects.Reverse();
        }


        string dateFormat = "ddd. dd/MM/yyyy";

        string timeModePeriodName;

        if (timePeriod == "custom" && timeMode == "custom")
        {
            timeModePeriodName = "Custom period";
        }
        else
        {
            timeModePeriodName = $"{timeMode.Substring(0, 1).ToUpper()}{timeMode.Substring(1)} {timePeriod.Substring(0, 1).ToUpper()}{timePeriod.Substring(1)}";
        }

        string timespanEndDate = endDate.HasValue ? endDate.Value.ToString(dateFormat, CultureInfo.InvariantCulture) : "DefaultEndDate";

        if (endDate.HasValue)
        {
            timespanEndDate = endDate.Value.ToString(dateFormat, CultureInfo.InvariantCulture);
        }
        string timespan = $"{startDate.ToString(dateFormat, CultureInfo.InvariantCulture)} - {timespanEndDate}";

        TimeModePeriodObject timeModePeriodObject = new TimeModePeriodObject(
            timeModePeriodName,
            timespan,
            total);

        TimePeriodObject timePeriodObject = new TimePeriodObject(groupByObjects);

        if (setDefault)
        {
            await SetDefaultQuery(queryString, token);
        }

        return new OverviewResponse(queryString, timeModePeriodObject, timePeriodObject);
    }

    public async Task<string> GetDefaultQuery(string token)
    {
        var user = await loginService.ValidateToken(token);

        var consultant = await dbContext.Consultants.FirstOrDefaultAsync(c => c.Id == user.Id);

        if (consultant is null)
        {
            throw new ArgumentException("Consultant not found");
        }

        var defaultQuery = consultant.DefaultQuery;

        if (string.IsNullOrEmpty(defaultQuery))
        {
            defaultQuery = "?sort_asc=false&show_days_no_records=false&set_default=false&start_date=null&end_date=null&time_period=week&time_mode=current&group_by=day&then_by=none";
        }

        return defaultQuery;
    }
    private DateTime FindStartDateOfTimePeriod(DateTime startDate, string timePeriod, string timeMode)
    {
        if (timePeriod == "day")
        {
            switch (timeMode)
            {
                case "last":
                    return LastValidDate(startDate.AddDays(-1));

                case "current":
                case "rolling":
                    return LastValidDate(startDate);
                default: throw new ArgumentException("Invalid timemode: " + timeMode);
            }
        }

        if (timePeriod == "week")
        {
            switch (timeMode)
            {
                case "last":
                    return FindStartDateOfWeek(startDate).AddDays(-7);

                case "current":
                    return FindStartDateOfWeek(startDate);

                case "rolling":
                    startDate = LastValidDate(startDate);
                    startDate = startDate.AddDays(-7);
                    return startDate;
                default: throw new ArgumentException("Invalid timemode: " + timeMode);
            }
        }

        if (timePeriod == "month")
        {
            switch (timeMode)
            {
                case "last":
                    startDate = FindStartDateOfMonth(startDate).AddMonths(-1);
                    return startDate;
                case "current":
                    startDate = FindStartDateOfMonth(startDate);
                    return startDate;

                case "rolling":
                    startDate = LastValidDate(startDate);
                    return startDate.AddMonths(-1);
                default: throw new ArgumentException("Invalid timemode: " + timeMode);
            }
        }

        if (timePeriod == "year")
        {
            switch (timeMode)
            {
                case "last":
                    return startDate.AddDays(-(startDate.DayOfYear - 1)).AddYears(-1);
                case "current":
                    return startDate.AddDays(-(startDate.DayOfYear - 1));
                case "rolling":
                    return startDate.AddYears(-1);
                default: throw new ArgumentException("Invalid timemode: " + timeMode);
            }
        }

        if (timePeriod == "custom")
        {
            return startDate;
        }

        throw new ArgumentException("Invalid timeperiod: " + timePeriod);
    }

    private DateTime FindStartDateOfWeek(DateTime startDate)
    {
        switch (startDate.DayOfWeek)
        {
            case DayOfWeek.Monday:
                return startDate;

            case DayOfWeek.Tuesday:
                return startDate.AddDays(-1);

            case DayOfWeek.Wednesday:
                return startDate.AddDays(-2);

            case DayOfWeek.Thursday:
                return startDate.AddDays(-3);

            case DayOfWeek.Friday:
                return startDate.AddDays(-4);

            case DayOfWeek.Saturday:
                return startDate.AddDays(-5);

            case DayOfWeek.Sunday:
                return startDate.AddDays(-6);

            default:
                throw new Exception("Error in day of week");
        }
    }

    private DateTime FindStartDateOfMonth(DateTime startDate)
    {
        return new DateTime(startDate.Year, startDate.Month, 1);
    }

    private DateTime LastValidDate(DateTime dateTime)
    {
        if (dateTime.DayOfWeek == DayOfWeek.Sunday || dateTime.DayOfWeek == DayOfWeek.Saturday)
        {
            return LastValidDate(dateTime.AddDays(-1));
        }
        else
        {
            return dateTime;
        }
    }

    private DateTime NextValidDate(DateTime startDate)
    {
        if (startDate.DayOfWeek == DayOfWeek.Sunday)
        {
            startDate = startDate.AddDays(1);
        }
        else if (startDate.DayOfWeek == DayOfWeek.Saturday)
        {
            startDate = startDate.AddDays(2);
        }

        return startDate;
    }

    private DateTime FindEndDate(DateTime originalStartDate, DateTime startDate, string timePeriod, string timeMode)
    {
        switch (timePeriod)
        {
            case "day":
                if (timeMode == "last" || timeMode == "current" || timeMode == "rolling")
                {
                    return LastValidDate(startDate);
                }
                else
                {
                    throw new ArgumentException($"Invalid timeMode: {timeMode}");
                }
            case "week":
                if (timeMode == "last" || timeMode == "current")
                {
                    return startDate.AddDays(6);
                }
                else if (timeMode == "rolling")
                {
                    return originalStartDate;
                }
                else
                {
                    throw new ArgumentException($"Invalid timeMode: {timeMode}");
                }
            case "month":
                if (timeMode == "last")
                {
                    return startDate.AddDays(DateTime.DaysInMonth(startDate.Year, startDate.Month) - 1);
                }
                else if (timeMode == "current")
                {
                    int endOfMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                    int diff = endOfMonth - startDate.Day;
                    return startDate.AddDays(diff);
                }
                else if (timeMode == "rolling")
                {
                    return originalStartDate;
                }
                else
                {
                    throw new ArgumentException($"Invalid timeMode: {timeMode}");
                }
            case "year":
                if (timeMode == "last" || timeMode == "current")
                {
                    return startDate.AddYears(1).AddDays(-1);
                }
                else if (timeMode == "rolling")
                {
                    return originalStartDate;
                }
                else
                {
                    throw new ArgumentException($"Invalid timeMode: {timeMode}");
                }
            default:
                throw new ArgumentException($"Incorrect timeperiod input: {timePeriod}");
        }
    }

    private async Task<List<GroupByObject>> GetGroupByObjects(string groupBy, ConsultantDto user, DateTime utcStartDate,
        DateTime utcEndDate, bool showDaysWithNoRecords)
    {
        List<GroupByObject> result = new List<GroupByObject>();

        var consultantCustomerIds = await dbContext.ConsultantCustomers
            .Where(cc => cc.ConsultantId == user.Id)
            .Select(cc => cc.Id)
            .ToListAsync();

        var rawData = await dbContext.TimeRegistrations
            .Where(tr => consultantCustomerIds.Contains(tr.ConsultantCustomerId)
                         && tr.RegistrationDate >= utcStartDate
                         && tr.RegistrationDate <= utcEndDate)
            .OrderBy(tr => tr.RegistrationDate).Select(tr => new
            {
                tr.RegistrationDate,
                tr.Hours
            })
            .ToListAsync();

        switch (groupBy)
        {
            case "day":

                result = await dbContext.TimeRegistrations
                .Join(dbContext.ConsultantCustomers,
                    tr => tr.ConsultantCustomerId,
                    cc => cc.Id,
                    (tr, cc) => new { tr, cc })
                .Where(joined => consultantCustomerIds.Contains(joined.cc.Id)
                                 && joined.tr.RegistrationDate >= utcStartDate
                                 && joined.tr.RegistrationDate <= utcEndDate)
                .GroupBy(joined => joined.tr.RegistrationDate.Date)
                .Select(group => new GroupByObject(
                    group.Key.ToString("dd/MM/yyyy"),
                    group.Sum(item => item.tr.Hours),
                    new List<ThenByObject>()
                ))
                .ToListAsync();

                if (showDaysWithNoRecords)
                {
                    var allDates = Enumerable.Range(0, 1 + utcEndDate.Subtract(utcStartDate).Days)
                    .Select(offset => utcStartDate.AddDays(offset))
                    .Select(date => date.ToString("dd/MM/yyyy"));

                    var missingDates = allDates.Except(result.Select(r => r.Name));

                    var missingGroupByObjects = missingDates.Select(date => new GroupByObject(date, 0, new List<ThenByObject>()));

                    result.AddRange(missingGroupByObjects);
                    result = result.OrderBy(r => DateTime.ParseExact(r.Name, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList();
                }

                break;
            case "week":

                result = rawData
                .GroupBy(tr => new
                {
                    Year = tr.RegistrationDate.Year,
                    Week = GetDanishWeekNumber(tr.RegistrationDate)
                })
                .Select(g => new GroupByObject(
                    $"Week {g.Key.Week}, {g.Key.Year}",
                    g.Sum(item => item.Hours),
                    new List<ThenByObject>()
                ))
                .ToList();

                if (showDaysWithNoRecords /*&& thenBy == "day"*/)
                {
                    // TODO: Implement logic if ThenBy == "day"
                }


                break;
            case "month":

                result = rawData
                .GroupBy(tr => new { tr.RegistrationDate.Year, tr.RegistrationDate.Month })
                .Select(g => new GroupByObject(
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month) + $" {g.Key.Year}",
                    g.Sum(tr => tr.Hours),
                    new List<ThenByObject>()
                ))
                .ToList();

                if (showDaysWithNoRecords /*&& thenBy == "day"*/)
                {
                    // TODO: Implement logic if ThenBy == "day"
                }

                break;
            case "year":

                result = rawData
                .GroupBy(tr => new { tr.RegistrationDate.Year })
                .Select(g => new GroupByObject(
                    g.Key.Year.ToString(),
                    g.Sum(tr => tr.Hours),
                    new List<ThenByObject>()
                ))
                .ToList();

                if (showDaysWithNoRecords /*&& thenBy == "day"*/)
                {
                    // TODO: Implement logic if ThenBy == "day"
                }

                break;
            case "client":

                result = await dbContext.TimeRegistrations
                .Join(dbContext.ConsultantCustomers,
                    tr => tr.ConsultantCustomerId,
                    cc => cc.Id,
                    (tr, cc) => new { tr, cc })
                .Join(dbContext.Customers,
                    joined => joined.cc.CustomerId,
                    c => c.Id,
                    (joined, c) => new { joined.tr, joined.cc, c })
                .Where(joined => consultantCustomerIds.Contains(joined.cc.Id)
                                 && joined.tr.RegistrationDate >= utcStartDate
                                 && joined.tr.RegistrationDate <= utcEndDate)
                .GroupBy(joined => joined.c.Name)
                .Select(group => new GroupByObject(
                    group.Key,
                    group.Sum(item => item.tr.Hours),
                    new List<ThenByObject>()
                ))
                .ToListAsync();


                if (showDaysWithNoRecords /*&& thenBy == "day"*/)
                {
                    // TODO: Implement logic if ThenBy == "day"
                }

                break;
        }


        return result;
    }
    private bool IsGroupByValid(string groupBy, DateTime startDate, DateTime endDate)
    {
        switch (groupBy)
        {
            case "day":
                return true;
            case "week":
                if (GetDanishWeekNumber(startDate) != GetDanishWeekNumber(endDate))
                {
                    return true;
                }
                else if (startDate.Year != endDate.Year)
                {
                    return true;
                }
                else if (startDate.Month != endDate.Month && startDate.Year == endDate.Year && GetDanishWeekNumber(startDate) == GetDanishWeekNumber(endDate))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case "month":
                if (startDate.Month != endDate.Month)
                {
                    return true;
                }
                else if (startDate.Year != endDate.Year)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case "year":
                if (startDate.Year != endDate.Year)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case "client":
                return true;
            default:
                return false;
        }
    }

    private int GetDanishWeekNumber(DateTime dateTime)
    {
        CultureInfo danishCulture = new CultureInfo("da-DK");
        Calendar calendar = danishCulture.Calendar;
        int weekNumber = calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return weekNumber;
    }

    private async Task SetDefaultQuery(string queryString, string token)
    {
        string newDefaultURL = "?" + queryString.Replace(" ", "").Replace("set_default=True", "set_default=False").Replace(",", "&");

        var user = await loginService.ValidateToken(token);

        var consultant = await dbContext.Consultants.FirstOrDefaultAsync(c => c.Id == user.Id);

        if (consultant is null)
        {
            throw new ArgumentException("Consultant not found");
        }
        else if (consultant.Id == user.Id)
        {
            consultant.DefaultQuery = newDefaultURL;
            dbContext.SaveChanges();
        }
        else
        {
            throw new ArgumentException("Consultant not found but wasn't null");
        }
        Console.WriteLine(consultant.DefaultQuery);
    }
}
