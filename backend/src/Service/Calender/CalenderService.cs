using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Persistence;
using Service.Login;
using Shared;
using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;


namespace Infrastructure
{
    public class CalenderService : ICalenderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://api.sallinggroup.com";
        private readonly string _bearerToken = "c0825d79-42ef-418e-b224-931d714be77b";

        public CalenderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);
        }

        public async Task<int> HolidaysAndWeekendDatesInTimeSpan(DateTime startDate, DateTime endDate)
        {
            var nationalHolidays = await GetOnlyNationalHolidays(startDate, endDate);
            var weekendDates = GetWeekends(startDate, endDate);
            var combinedDates = new HashSet<DateTime>(weekendDates);

            if (nationalHolidays is not null)
            {
                foreach (var holiday in nationalHolidays)
                {
                    combinedDates.Add(holiday.Date);
                }
            }

            if (combinedDates is null)
            {
                return 0;
            }

            return combinedDates.Count;
        }

        // Method to find holidays between to chosen dates. Method will also include holidays that are NOT national holidays (IE Xmas Eve)
        public async Task<ICollection<Holiday>?> GetHolidays(DateTime startDate, DateTime endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/v1/holidays?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&translation=en-us");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var holidayObjects = JsonConvert.DeserializeObject<List<Holiday>>(content);

                return holidayObjects;
            }
            catch (Exception e)
            {
                throw new Exception($"Unexpected exception: {e.Message}");
            }
        }

        public async Task<ICollection<Holiday>?> GetOnlyNationalHolidays(DateTime startDate, DateTime endDate)
        {
            var holidays = await GetHolidays(startDate, endDate);

            if (holidays is null)
            {
                return null;
            }

            var nationalHolidays = new List<Holiday>();
            foreach (var holiday in holidays)
            {
                if (holiday.NationalHoliday)
                {
                    nationalHolidays.Add(holiday);
                }
            }

            return nationalHolidays;
        }

        public async Task<bool> IsDateValid(DateTimeOffset date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            var nationalHolidays = await GetOnlyNationalHolidays(date.Date, date.Date);

            if (nationalHolidays is null || nationalHolidays.Count == 0)
            {
                return true;
            }

            return false;
        }

        // Static Date and Time methods

        public static DateTimeOffset SetMaxTimeOnDate(DateTimeOffset date)
        {
            var latestTime = new TimeSpan(23, 59, 59, 999, 9999);
            var maxDateTime = date.Date + latestTime;
            var result = new DateTimeOffset(maxDateTime, date.Offset);
            Console.WriteLine($"SetMaxTimeOnDate: Date was set to {result}");

            return result;
        }

        public static DateTimeOffset SetMinTimeOnDate(DateTimeOffset date)
        {
            var earliestTime = new TimeSpan(0, 0, 0);
            var minDateTime = date.Date + earliestTime;
            var result = new DateTimeOffset(minDateTime, date.Offset);
            Console.WriteLine($"SetMinTimeOnDate: Date was set to {result}");

            return result;
        }

        public static DateTimeOffset FindStartDateOfTimePeriod(DateTimeOffset startDate, string timePeriod, string timeMode)
        {
            if (timePeriod == "day")
            {
                switch (timeMode)
                {
                    case "last":
                        return startDate.AddDays(-1);

                    case "current":
                        return startDate;

                    case "rolling":
                        return startDate;

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

        public static DateTimeOffset FindStartDateOfWeek(DateTimeOffset startDate)
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

        public static DateTimeOffset FindStartDateOfMonth(DateTimeOffset startDate)
        {
            return new DateTimeOffset(startDate.Year, startDate.Month, 1, 0, 0, 0, startDate.Offset);
        }

        public static DateTimeOffset FindEndDate(DateTimeOffset startDate, string timePeriod, string timeMode)
        {
            switch (timePeriod)
            {
                case "day":
                    if (timeMode == "last" || timeMode == "rolling")
                    {
                        return startDate;
                    }
                    else if (timeMode == "current")
                    {
                        return DateTimeOffset.Now;
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
                        return DateTimeOffset.Now;
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
                        return DateTimeOffset.Now;
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
                        return DateTimeOffset.Now;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid timeMode: {timeMode}");
                    }
                default:
                    throw new ArgumentException($"Incorrect timeperiod input: {timePeriod}");
            }
        }

        public static List<DateTime> GetWeekends(DateTime startDate, DateTime endDate)
        {
            var weekendDates = new List<DateTime>();
            TimeSpan timeSpan = endDate - startDate;
            int days = (int)timeSpan.TotalDays;

            for (int i = 0; i <= days; i++)
            {
                DateTime date = startDate.AddDays(i);
                if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                {
                    weekendDates.Add(date);
                }
            }

            return weekendDates;
        }

        public static int GetDanishWeekNumber(DateTimeOffset dateTimeOffset)
        {
            CultureInfo danishCulture = new("da-DK");
            Calendar calendar = danishCulture.Calendar;

            int weekNumber = calendar.GetWeekOfYear(dateTimeOffset.DateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNumber;
        }

        public static readonly List<string> TimeUnitOrder = ["day", "week", "month", "year"];

        // Legacy methods

        private DateTimeOffset LastValidDate(DateTimeOffset dateTime)
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

        private DateTimeOffset NextValidDate(DateTimeOffset dateTime)
        {
            if (dateTime.DayOfWeek == DayOfWeek.Sunday || dateTime.DayOfWeek == DayOfWeek.Saturday)
            {
                return NextValidDate(dateTime.AddDays(-1));
            }
            else
            {
                return dateTime;
            }
        }
    }
}
