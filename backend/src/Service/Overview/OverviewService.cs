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
        private static readonly List<string> TimeUnitOrder = ["day", "week", "month", "year"];

        public OverviewService(LogpunchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Work

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
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(startDate, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(CalenderService.FindEndDate(originalStartDate, startDate, timePeriod, timeMode));
            }
            else if (timePeriod == "custom" && endDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(startDate, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(DateTimeOffset.Now);
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

        // Transportation

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
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(startDate, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(CalenderService.FindEndDate(originalStartDate, startDate, timePeriod, timeMode));
            }
            else if (timePeriod == "custom" && endDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(startDate, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(DateTimeOffset.Now);
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

        // Absence

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
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(startDate, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(CalenderService.FindEndDate(originalStartDate, startDate, timePeriod, timeMode));
            }
            else if (timePeriod == "custom" && endDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(startDate, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(DateTimeOffset.Now);
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

        // Helper methods

        private async Task<List<GroupByObject>> GetGroupByObjects(string groupBy, LogpunchUser user, DateTimeOffset startDate, DateTimeOffset endDate, bool showDaysWithNoRecords, RegistrationType registrationType, string thenBy)
        {
            List<GroupByObject> result = new List<GroupByObject>();

            var employeeClientRelationIds = await _dbContext.EmployeeClientRelations
                .Where(ecr => ecr.EmployeeId == user.Id)
                .Select(ecr => ecr.ClientId)
                .ToListAsync();

            var rawData = await _dbContext.Registrations
                .Where(r => r.CorrectionOfId == null
                            && r.Type == registrationType
                            && r.EmployeeId == user.Id
                            && r.Start >= startDate
                            && r.Start <= endDate)
                .OrderBy(r => r.Start)
                .ToListAsync();

            // Log rawData count
            Console.WriteLine($"Raw Data Count: {rawData.Count}");

            var corrections = await _dbContext.Registrations
                .Where(r => r.CorrectionOfId != null
                            && r.Type == registrationType
                            && r.EmployeeId == user.Id
                            && r.Start >= startDate
                            && r.Start <= endDate)
                .OrderBy(r => r.Start)
                .ToListAsync();

            var mostRecentCorrections = corrections
                .GroupBy(r => r.CorrectionOfId)
                .Select(g => g.OrderByDescending(r => r.CreationTime).First())
                .ToList();

            var finalData = new List<LogpunchRegistrationDto>();

            foreach (var registration in rawData)
            {
                var mostRecentCorrection = mostRecentCorrections.FirstOrDefault(r => r.CorrectionOfId == registration.Id);
                if (mostRecentCorrection is null)
                {
                    finalData.Add(new LogpunchRegistrationDto(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId,
                    registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId));
                }
                else
                {
                    finalData.Add(new LogpunchRegistrationDto(mostRecentCorrection.Id, mostRecentCorrection.EmployeeId, mostRecentCorrection.Type.ToString(), mostRecentCorrection.Amount, mostRecentCorrection.Start, mostRecentCorrection.End, mostRecentCorrection.CreatorId,
                    mostRecentCorrection.ClientId, mostRecentCorrection.CreationTime, mostRecentCorrection.Status.ToString(), mostRecentCorrection.FirstComment, mostRecentCorrection.SecondComment, null));
                }
            }

            // Log correctedData count
            Console.WriteLine($"Corrected Data Count: {finalData.Count}");

            result = groupBy switch
            {
                "day" => GroupByDay(finalData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "week" => GroupByWeek(finalData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "month" => GroupByMonth(finalData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "year" => GroupByYear(finalData, showDaysWithNoRecords, startDate, endDate, thenBy),
                "client" => await GroupByClient(finalData, employeeClientRelationIds, startDate, endDate, thenBy),
                _ => throw new ArgumentException($"Invalid groupby option selected: {groupBy}")
            };

            return result;
        }

        private List<ThenByObject> GetThenByObjects(IEnumerable<LogpunchRegistrationDto> group, string thenBy, string groupBy)
        {
            // Check if thenBy is a valid time unit and is larger or equal to groupBy
            if (TimeUnitOrder.Contains(thenBy) && TimeUnitOrder.Contains(groupBy))
            {
                if (TimeUnitOrder.IndexOf(thenBy) >= TimeUnitOrder.IndexOf(groupBy))
                {
                    // If thenBy is equal or larger than groupBy, adjust thenBy to be one level smaller than groupBy
                    int groupByIndex = TimeUnitOrder.IndexOf(groupBy);
                    thenBy = groupByIndex > 0 ? TimeUnitOrder[groupByIndex - 1] : groupBy;
                }
            }

            return thenBy switch
            {
                "day" => group
                    .GroupBy(g => g.Start.Date)
                    .Select(g => new ThenByObject(g.Key.ToString("dd/MM/yyyy"), g.Sum(i => i.Amount ?? 0)))
                    .ToList(),
                "week" => group
                    .GroupBy(g => new { g.Start.Year, Week = CalenderService.GetDanishWeekNumber(g.Start) })
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
                    .GroupBy(g => g.ClientId)
                    .Select(g =>
                    {
                        string clientName = GetClientName(g.Key);
                        return new ThenByObject(clientName, g.Sum(i => i.Amount ?? 0));
                    })
                    .ToList(),
                _ => new List<ThenByObject>()
            };
        }

        private List<GroupByObject> GroupByDay(List<LogpunchRegistrationDto> correctedData, bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => r.Start.Date)
                .Select(group => new GroupByObject(
                    group.Key.ToString("dd/MM/yyyy"),
                    group.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(group, thenBy, "day")
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

        private List<GroupByObject> GroupByWeek(List<LogpunchRegistrationDto> correctedData, bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year, Week = CalenderService.GetDanishWeekNumber(r.Start) })
                .Select(g => new GroupByObject(
                    $"Week {g.Key.Week}, {g.Key.Year}",
                    g.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(g, thenBy, "week")
                ))
                .ToList();

            return result;
        }

        private List<GroupByObject> GroupByMonth(List<LogpunchRegistrationDto> correctedData, bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year, r.Start.Month })
                .Select(g => new GroupByObject(
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month) + $" {g.Key.Year}",
                    g.Sum(r => r.Amount ?? 0),
                    GetThenByObjects(g, thenBy, "month")
                ))
                .ToList();

            return result;
        }

        private List<GroupByObject> GroupByYear(List<LogpunchRegistrationDto> correctedData, bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year })
                .Select(g => new GroupByObject(
                    g.Key.Year.ToString(),
                    g.Sum(r => r.Amount ?? 0),
                    GetThenByObjects(g, thenBy, "year")
                ))
                .ToList();

            return result;
        }

        private async Task<List<GroupByObject>> GroupByClient(List<LogpunchRegistrationDto> correctedData, List<Guid> employeeClientRelationIds, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
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
                    joined.r.EmployeeId,
                    joined.r.Type,
                    joined.r.Amount,
                    joined.r.Start,
                    joined.r.End,
                    joined.r.CreatorId,
                    joined.r.ClientId,
                    joined.r.CreationTime,
                    joined.r.Status,
                    joined.r.FirstComment,
                    joined.r.SecondComment,
                    joined.r.CorrectionOfId,
                    ClientName = joined.c.Name ?? "No Client"
                })
                .ToListAsync();

            var correctedGroupedData = new List<(Guid Id, Guid EmployeeId, string Type, int? Amount, DateTimeOffset Start, DateTimeOffset? End, Guid CreatorId, Guid? ClientId, DateTimeOffset CreationTime, string Status, string? FirstComment, string? SecondComment, Guid? CorrectionOfId, string ClientName)>();

            foreach (var data in groupedData)
            {
                var correction = await GetMostRecentCorrection(data.Id);
                if (correction is not null)
                {
                    correctedGroupedData.Add((data.Id, data.EmployeeId, data.Type.ToString(), correction.Amount, correction.Start, correction.End, data.CreatorId, correction.ClientId, data.CreationTime, data.Status.ToString(), correction.FirstComment, correction.SecondComment, null, data.ClientName));
                }
                else
                {
                    correctedGroupedData.Add((data.Id, data.EmployeeId, data.Type.ToString(), data.Amount, data.Start, data.End, data.CreatorId, data.ClientId, data.CreationTime, data.Status.ToString(), data.FirstComment, data.SecondComment, data.CorrectionOfId, data.ClientName));
                }
            }

            var result = correctedGroupedData
                .GroupBy(g => g.ClientName)
                .Select(group => new GroupByObject(
                    group.Key,
                    group.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(group.Select(x => new LogpunchRegistrationDto(
                        x.Id,
                        x.EmployeeId,
                        x.Type,
                        x.Amount,
                        x.Start,
                        x.End,
                        x.CreatorId,
                        x.ClientId,
                        x.CreationTime,
                        x.Status,
                        x.FirstComment,
                        x.SecondComment,
                        x.CorrectionOfId)), thenBy, "client")
                ))
                .ToList();

            return result;
        }

        private static bool IsGroupByValid(string groupBy, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            switch (groupBy)
            {
                case "day":
                    return true;
                case "week":
                    if (CalenderService.GetDanishWeekNumber(startDate) != CalenderService.GetDanishWeekNumber(endDate))
                    {
                        return true;
                    }
                    else if (startDate.Year != endDate.Year)
                    {
                        return true;
                    }
                    else if (startDate.Month != endDate.Month && startDate.Year == endDate.Year && CalenderService.GetDanishWeekNumber(startDate) == CalenderService.GetDanishWeekNumber(endDate))
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

        private string GetClientName(Guid? clientId)
        {
            if (clientId.HasValue)
            {
                var client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId.Value);
                return client?.Name ?? "Unknown Client";
            }
            else
            {
                return "No Client";
            }
        }
        private async Task<LogpunchRegistration?> GetMostRecentCorrection(Guid registrationId)
        {
            return await _dbContext.Registrations
                .Where(r => r.CorrectionOfId == registrationId)
                .OrderByDescending(r => r.CreationTime)
                .FirstOrDefaultAsync();
        }
    }
}
