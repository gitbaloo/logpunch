using System.Formats.Asn1;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Persistence;
using Service.Login;
using Shared;

namespace Infrastructure
{
    public class OverviewService : IOverviewService
    {
        private readonly LogpunchDbContext _dbContext;

        public OverviewService(LogpunchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LogpunchRegistrationDto?> GetOngoingRegistration(Guid userId, Guid? employeeId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId is null)
            {
                employee = user;
            }
            else if (employeeId is not null && user.Role == UserRole.Admin)
            {
                employee = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == employeeId.Value) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                throw new InvalidOperationException("You can only look at your own ongoing registration");
            }

            var ongoingRegistration = await _dbContext.Registrations.FirstOrDefaultAsync(or =>
                or.Status == RegistrationStatus.Ongoing && or.EmployeeId == employee.Id && or.CorrectionOfId == null && or.Type == RegistrationType.Work);

            if (ongoingRegistration is null)
            {
                return null;
            }

            var registrationDto = new LogpunchRegistrationDto(
                ongoingRegistration.Id,
                ongoingRegistration.EmployeeId,
                ongoingRegistration.Type.ToString(),
                ongoingRegistration.Amount,
                ongoingRegistration.Start,
                ongoingRegistration.End,
                ongoingRegistration.CreatorId,
                ongoingRegistration.ClientId,
                ongoingRegistration.CreationTime,
                ongoingRegistration.Status.ToString(),
                ongoingRegistration.FirstComment,
                ongoingRegistration.SecondComment,
                ongoingRegistration.CorrectionOfId);

            return registrationDto;
        }

        public async Task<List<LogpunchRegistrationDto>> GetUnsettledWorkRegistrations(Guid userId, Guid? employeeId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId is null)
            {
                employee = user;
            }
            else if (employeeId is not null && user.Role == UserRole.Admin)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                throw new InvalidOperationException("You can only see your own registrations");
            }

            List<LogpunchRegistration> registrations = _dbContext.Registrations.Where(r => r.EmployeeId == employeeId && r.Type == RegistrationType.Work && r.CorrectionOfId == null).Where(r => r.Status != RegistrationStatus.Ongoing || r.Status != RegistrationStatus.Settled).ToList();

            List<LogpunchRegistrationDto> registrationDtos = [];

            foreach (var registration in registrations)
            {
                var correction = await GetMostRecentCorrection(registration.Id);

                if (correction is not null)
                {
                    LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), correction.Amount, correction.Start, correction.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), correction.FirstComment, correction.SecondComment, null);
                    registrationDtos.Add(registrationDto);
                }
                else
                {
                    LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, null);
                    registrationDtos.Add(registrationDto);
                }
            }

            return registrationDtos;
        }

        public async Task<OverviewResponse> WorkOverviewQuery(Guid userId, Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords, bool setDefault,
                DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy)
        {
            RegistrationType registrationType = RegistrationType.Work;
            DateTimeOffset originalStartDate = startDate;
            DateTimeOffset nonNullableEndDate = default;
            string queryString;
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId is null || employeeId == user.Id)
            {
                employee = user;
            }
            else if (employeeId is not null && user.Role == UserRole.Admin)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                throw new InvalidOperationException("You cannot look at another employees overview if you aren't an admin");
            }


            if (endDate is null)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, set_default={setDefault}, start_date=null, end_date={endDate}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }
            else
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, set_default={setDefault}, start_date={startDate.DateTime.ToShortDateString()}, end_date={endDate.Value.DateTime.ToShortDateString()}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }

            if (employee is null)
            {
                throw new ArgumentNullException(nameof(employee));
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
            else if (timePeriod == "custom" && endDate is null)
            {
                startDate = FindStartDateOfTimePeriod(startDate, timePeriod, timeMode);
                endDate = DateTimeOffset.Now;
            }

            if (endDate.HasValue)
            {
                nonNullableEndDate = endDate.Value;
            }

            Console.WriteLine($"Start date: {startDate} End date: {endDate}");


            if (!IsGroupByValid(groupBy, startDate, nonNullableEndDate))
            {
                throw new ArgumentException($"Invalid groupby option selected: A time period can only be grouped into period units that it contains more than one of. startDate: {startDate} endDate: {endDate}");
            }

            var groupByObjects = await GetGroupByObjects(groupBy, employee, startDate, nonNullableEndDate, showDaysWithNoRecords, registrationType, thenBy);
            int total = 0;

            foreach (var groupByObject in groupByObjects)
            {
                total += groupByObject.Total ?? 0;
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

            TimeModePeriodObject timeModePeriodObject = new(
                timeModePeriodName,
                timespan,
                total);

            TimePeriodObject timePeriodObject = new(groupByObjects);

            if (setDefault)
            {
                SetDefaultQuery(queryString, employee);
            }

            return new OverviewResponse(registrationType.ToString(), queryString, timeModePeriodObject, timePeriodObject);
        }

        public async Task<string> GetDefaultWorkQuery(Guid userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");

            if (user is null)
            {
                throw new ArgumentException("User not found");
            }

            var defaultQuery = user.DefaultQuery;

            if (string.IsNullOrEmpty(defaultQuery))
            {
                defaultQuery = "?sort_asc=false&show_days_no_records=false&set_default=false&start_date=null&end_date=null&time_period=week&time_mode=current&group_by=day&then_by=none";
            }

            return defaultQuery;
        }

        public async Task<List<LogpunchRegistrationDto>> GetUnsettledAbsenceRegistrations(Guid userId, Guid? employeeId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId is null)
            {
                employee = user;
            }
            else if (employeeId is not null && user.Role == UserRole.Admin)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                throw new InvalidOperationException("You can only see your own registrations");
            }

            List<LogpunchRegistration> registrations = _dbContext.Registrations.Where(r => r.EmployeeId == employeeId && r.Type != RegistrationType.Work && r.Type != RegistrationType.Transportation && r.CorrectionOfId == null).Where(r => r.Status != RegistrationStatus.Settled).ToList();

            List<LogpunchRegistrationDto> registrationDtos = [];

            foreach (var registration in registrations)
            {
                var correction = await GetMostRecentCorrection(registration.Id);

                if (correction is not null)
                {
                    LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), correction.Amount, correction.Start, correction.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), correction.FirstComment, correction.SecondComment, null);
                    registrationDtos.Add(registrationDto);
                }
                else
                {
                    LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, null);
                    registrationDtos.Add(registrationDto);
                }
            }

            return registrationDtos;
        }

        public async Task<List<LogpunchRegistrationDto>> GetUnsettledTransportationRegistrations(Guid userId, Guid? employeeId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId is null)
            {
                employee = user;
            }
            else if (employeeId is not null && user.Role == UserRole.Admin)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                throw new InvalidOperationException("You can only see your own registrations");
            }

            List<LogpunchRegistration> registrations = _dbContext.Registrations.Where(r => r.EmployeeId == employeeId && r.Type == RegistrationType.Transportation && r.CorrectionOfId == null).Where(r => r.Status != RegistrationStatus.Settled).ToList();

            List<LogpunchRegistrationDto> registrationDtos = [];

            foreach (var registration in registrations)
            {
                var correction = await GetMostRecentCorrection(registration.Id);

                if (correction is not null)
                {
                    LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), correction.Amount, correction.Start, correction.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), correction.FirstComment, correction.SecondComment, null);
                    registrationDtos.Add(registrationDto);
                }
                else
                {
                    LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, null);
                    registrationDtos.Add(registrationDto);
                }
            }

            return registrationDtos;
        }

        public async Task<OverviewResponse> AbsenceOverviewQuery(Guid userId, Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords,
                DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy, string absenceType)
        {
            RegistrationType registrationType = RegistrationTypeConverter.ConvertStringToEnum(absenceType);

            if (registrationType == RegistrationType.Work || registrationType == RegistrationType.Transportation)
            {
                throw new InvalidOperationException($"{registrationType} is not among the absence types. These are '{RegistrationType.Leave}', '{RegistrationType.Sickness} and {RegistrationType.Vacation}");
            }

            DateTimeOffset originalStartDate = startDate;
            DateTimeOffset nonNullableEndDate = default;
            string queryString;
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId is null)
            {
                employee = user;
            }
            else if (employeeId is not null && user.Role == UserRole.Admin)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                throw new InvalidOperationException("You cannot look at another employees overview");
            }

            if (endDate is null)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, start_date=null, end_date={endDate}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}, absence_type={absenceType}";
            }
            else
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, start_date={startDate.DateTime.ToShortDateString()}, end_date={endDate.Value.DateTime.ToShortDateString()}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }

            if (employee is null)
            {
                throw new ArgumentNullException(nameof(employee));
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

            if (groupBy == "client" || thenBy == "client")
            {
                throw new ArgumentException($"You cannot categorize your absence overview in 'clients'. GroupBy was {groupBy} and ThenBy was {thenBy}");
            }

            if (timePeriod != "custom" && endDate is null)
            {
                startDate = FindStartDateOfTimePeriod(startDate, timePeriod, timeMode);
                endDate = FindEndDate(originalStartDate, startDate, timePeriod, timeMode);
            }
            else if (timePeriod == "custom" && endDate is null)
            {
                startDate = FindStartDateOfTimePeriod(startDate, timePeriod, timeMode);
                endDate = DateTimeOffset.Now;
            }

            if (endDate.HasValue)
            {
                nonNullableEndDate = endDate.Value;
            }

            Console.WriteLine($"Start date: {startDate} End date: {endDate}");


            if (!IsGroupByValid(groupBy, startDate, nonNullableEndDate))
            {
                throw new ArgumentException($"Invalid groupby option selected: A time period can only be grouped into period units that it contains more than one of. startDate: {startDate} endDate: {endDate}");
            }

            var groupByObjects = await GetGroupByObjects(groupBy, employee, startDate, nonNullableEndDate, showDaysWithNoRecords, registrationType, thenBy);
            int total = 0;

            foreach (var groupByObject in groupByObjects)
            {
                total += groupByObject.Total ?? 0;
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

            TimeModePeriodObject timeModePeriodObject = new(
                timeModePeriodName,
                timespan,
                total);

            TimePeriodObject timePeriodObject = new(groupByObjects);

            return new OverviewResponse(registrationType.ToString(), queryString, timeModePeriodObject, timePeriodObject);
        }

        public async Task<OverviewResponse> TransportationOverviewQuery(Guid userId, Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords,
                DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy)
        {
            RegistrationType registrationType = RegistrationType.Transportation;
            DateTimeOffset originalStartDate = startDate;
            DateTimeOffset nonNullableEndDate = default;
            string queryString;
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId is null)
            {
                employee = user;
            }
            else if (employeeId is not null && user.Role == UserRole.Admin)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                throw new InvalidOperationException("You cannot look at another employees overview");
            }


            if (endDate is null)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, start_date=null, end_date={endDate}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }
            else
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showDaysWithNoRecords}, start_date={startDate.DateTime.ToShortDateString()}, end_date={endDate.Value.DateTime.ToShortDateString()}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }

            if (employee is null)
            {
                throw new ArgumentNullException(nameof(employee));
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
            else if (timePeriod == "custom" && endDate is null)
            {
                startDate = FindStartDateOfTimePeriod(startDate, timePeriod, timeMode);
                endDate = DateTimeOffset.Now;
            }

            if (endDate.HasValue)
            {
                nonNullableEndDate = endDate.Value;
            }

            Console.WriteLine($"Start date: {startDate} End date: {endDate}");


            if (!IsGroupByValid(groupBy, startDate, nonNullableEndDate))
            {
                throw new ArgumentException($"Invalid groupby option selected: A time period can only be grouped into period units that it contains more than one of. startDate: {startDate} endDate: {endDate}");
            }

            var groupByObjects = await GetGroupByObjects(groupBy, employee, startDate, nonNullableEndDate, showDaysWithNoRecords, registrationType, thenBy);
            int total = 0;

            foreach (var groupByObject in groupByObjects)
            {
                total += groupByObject.Total ?? 0;
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

            TimeModePeriodObject timeModePeriodObject = new(
                timeModePeriodName,
                timespan,
                total);

            TimePeriodObject timePeriodObject = new(groupByObjects);

            return new OverviewResponse(registrationType.ToString(), queryString, timeModePeriodObject, timePeriodObject);
        }

        private DateTimeOffset FindStartDateOfTimePeriod(DateTimeOffset startDate, string timePeriod, string timeMode)
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

        private DateTimeOffset FindStartDateOfWeek(DateTimeOffset startDate)
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

        private DateTimeOffset FindStartDateOfMonth(DateTimeOffset startDate)
        {
            return new DateTimeOffset(startDate.Year, startDate.Month, 1, 0, 0, 0, startDate.Offset);
        }

        private DateTimeOffset FindEndDate(DateTimeOffset originalStartDate, DateTimeOffset startDate, string timePeriod, string timeMode)
        {
            switch (timePeriod)
            {
                case "day":
                    if (timeMode == "last" || timeMode == "current" || timeMode == "rolling")
                    {
                        return startDate;
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

        private async Task<List<GroupByObject>> GetGroupByObjects(string groupBy, LogpunchUser user, DateTimeOffset startDate,
        DateTimeOffset endDate, bool showDaysWithNoRecords, RegistrationType registrationType, string thenBy)
        {
            List<GroupByObject> result = new List<GroupByObject>();

            var employeeClientRelationIds = await _dbContext.EmployeeClientRelations
                .Where(ecr => ecr.EmployeeId == user.Id)
                .Select(ecr => ecr.ClientId)
                .ToListAsync();

            var rawData = await _dbContext.Registrations
                .Where(r => (r.ClientId == null || employeeClientRelationIds.Contains(r.ClientId.Value))
                             && r.Type == registrationType
                             && r.Start >= startDate
                             && r.Start <= endDate)
                .OrderBy(r => r.Start)
                .Select(r => new
                {
                    r.Id,
                    r.Start,
                    r.Amount,
                    r.ClientId
                })
                .ToListAsync();

            var correctedData = new List<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId)>();

            foreach (var registration in rawData)
            {
                var correction = await GetMostRecentCorrection(registration.Id);
                if (correction is not null)
                {
                    correctedData.Add((registration.Id, correction.Start, correction.Amount, correction.ClientId));
                }
                else
                {
                    correctedData.Add((registration.Id, registration.Start, registration.Amount, registration.ClientId));
                }
            }

            result = groupBy switch
            {
                "day" => GroupByDay(correctedData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "week" => GroupByWeek(correctedData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "month" => GroupByMonth(correctedData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "year" => GroupByYear(correctedData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "client" => await GroupByClient(correctedData, employeeClientRelationIds, startDate, endDate, thenBy),
                _ => throw new ArgumentException($"Invalid groupby option selected: {groupBy}")
            };

            return result;
        }

        private List<GroupByObject> GroupByDay(List<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId)> correctedData,
            bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => r.Start.Date)
                .Select(group => new GroupByObject(
                    group.Key.ToString("dd/MM/yyyy"),
                    group.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(group, thenBy)
                ))
                .ToList();

            if (showDaysWithNoRecords)
            {
                var allDates = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => date.ToString("dd/MM/yyyy"));

                var missingDates = allDates.Except(result.Select(r => r.Name));

                var missingGroupByObjects = missingDates.Select(date => new GroupByObject(date, 0, new List<ThenByObject>()));

                result.AddRange(missingGroupByObjects);
                result = result.OrderBy(r => DateTimeOffset.ParseExact(r.Name, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList();
            }

            return result;
        }

        private List<GroupByObject> GroupByWeek(List<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId)> correctedData,
            bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year, Week = GetDanishWeekNumber(r.Start) })
                .Select(g => new GroupByObject(
                    $"Week {g.Key.Week}, {g.Key.Year}",
                    g.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(g.Select(x => (x.Id, x.Start, x.Amount, x.ClientId)), thenBy)
                ))
                .ToList();

            return result;
        }

        private List<GroupByObject> GroupByMonth(List<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId)> correctedData,
            bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year, r.Start.Month })
                .Select(g => new GroupByObject(
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month) + $" {g.Key.Year}",
                    g.Sum(r => r.Amount ?? 0),
                    GetThenByObjects(g.Select(x => (x.Id, x.Start, x.Amount, x.ClientId)), thenBy)
                ))
                .ToList();

            return result;
        }

        private List<GroupByObject> GroupByYear(List<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId)> correctedData,
            bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year })
                .Select(g => new GroupByObject(
                    g.Key.Year.ToString(),
                    g.Sum(r => r.Amount ?? 0),
                    GetThenByObjects(g.Select(x => (x.Id, x.Start, x.Amount, x.ClientId)), thenBy)
                ))
                .ToList();

            return result;
        }

        private async Task<List<GroupByObject>> GroupByClient(List<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId)> correctedData,
        List<Guid> employeeClientRelationIds, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var groupedData = await _dbContext.Registrations
                .Join(_dbContext.EmployeeClientRelations,
                    r => r.ClientId,
                    ecr => ecr.ClientId,
                    (r, ecr) => new { r, ecr })
                .Join(_dbContext.Clients,
                    joined => joined.ecr.ClientId,
                    c => c.Id,
                    (joined, c) => new { joined.r, joined.ecr, c })
                .Where(joined => (joined.r.ClientId == null || employeeClientRelationIds.Contains(joined.ecr.ClientId))
                                 && joined.r.Start >= startDate
                                 && joined.r.Start <= endDate)
                .Select(joined => new
                {
                    joined.r.Id,
                    joined.r.Start,
                    joined.r.Amount,
                    joined.r.ClientId,
                    ClientName = joined.c.Name ?? "No Client"
                })
                .ToListAsync();

            var correctedGroupedData = new List<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId, string ClientName)>();

            foreach (var data in groupedData)
            {
                var correction = await GetMostRecentCorrection(data.Id);
                if (correction is not null)
                {
                    correctedGroupedData.Add((data.Id, correction.Start, correction.Amount, correction.ClientId, data.ClientName));
                }
                else
                {
                    correctedGroupedData.Add((data.Id, data.Start, data.Amount, data.ClientId, data.ClientName));
                }
            }

            var result = correctedGroupedData
                .GroupBy(g => g.ClientName)
                .Select(group => new GroupByObject(
                    group.Key,
                    group.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(group.Select(x => (x.Id, x.Start, x.Amount, x.ClientId)), thenBy)
                ))
                .ToList();

            return result;
        }

        private List<ThenByObject> GetThenByObjects(IEnumerable<(Guid Id, DateTimeOffset Start, int? Amount, Guid? ClientId)> group, string thenBy)
        {
            return thenBy switch
            {
                "day" => group
                    .GroupBy(g => g.Start.Date)
                    .Select(g => new ThenByObject(g.Key.ToString("dd/MM/yyyy"), g.Sum(i => i.Amount ?? 0)))
                    .ToList(),
                "week" => group
                    .GroupBy(g => new { g.Start.Year, Week = GetDanishWeekNumber(g.Start) })
                    .Select(g => new ThenByObject($"Week {g.Key.Week}, {g.Key.Year}", g.Sum(i => i.Amount ?? 0)))
                    .ToList(),
                "month" => group
                    .GroupBy(g => new { g.Start.Year, g.Start.Month })
                    .Select(g => new ThenByObject(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month) + $" {g.Key.Year}", g.Sum(i => i.Amount ?? 0)))
                    .ToList(),
                "year" => group
                    .GroupBy(g => new { g.Start.Year })
                    .Select(g => new ThenByObject(g.Key.Year.ToString(), g.Sum(i => i.Amount ?? 0)))
                    .ToList(),
                "client" => group
                    .GroupBy(g => g.ClientId.ToString())
                    .Select(g => new ThenByObject(g.Key ?? "No Client", g.Sum(i => i.Amount ?? 0)))
                    .ToList(),
                _ => new List<ThenByObject>()
            };
        }
        private bool IsGroupByValid(string groupBy, DateTimeOffset startDate, DateTimeOffset endDate)
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

        private int GetDanishWeekNumber(DateTimeOffset dateTimeOffset)
        {
            CultureInfo danishCulture = new("da-DK");
            Calendar calendar = danishCulture.Calendar;

            int weekNumber = calendar.GetWeekOfYear(dateTimeOffset.DateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNumber;
        }

        private void SetDefaultQuery(string queryString, LogpunchUser user)
        {
            string newDefaultURL = "?" + queryString.Replace(" ", "").Replace("set_default=True", "set_default=False").Replace(",", "&");

            if (user is null)
            {
                throw new ArgumentException("User was null");
            }
            else
            {
                user.DefaultQuery = newDefaultURL;
                _dbContext.SaveChanges();
            }

            Console.WriteLine(user.DefaultQuery);
        }

        private async Task<LogpunchRegistration?> GetMostRecentCorrection(Guid registrationId)
        {
            LogpunchRegistration? registration = await _dbContext.Registrations.Where(r => r.CorrectionOfId == registrationId).OrderByDescending(r => r.CreationTime).FirstOrDefaultAsync();

            return registration;
        }
    }
}
