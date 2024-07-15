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

        public static DateTimeOffset SetMaxTimeOnDate(DateTimeOffset date)
        {
            var latestTime = new TimeSpan(23, 59, 59, 999, 9999);
            var maxDateTime = date.Date + latestTime;
            var result = new DateTimeOffset(maxDateTime, date.Offset);

            return result;
        }

        public static DateTimeOffset SetMinTimeOnDate(DateTimeOffset date)
        {
            var earliestTime = new TimeSpan(0, 0, 0);
            var minDateTime = date.Date + earliestTime;
            var result = new DateTimeOffset(minDateTime, date.Offset);

            return result;
        }

        private ICollection<DateTime> GetWeekends(DateTime startDate, DateTime endDate)
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
