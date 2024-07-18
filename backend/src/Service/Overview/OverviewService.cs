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

        // Work

        public async Task<LogpunchRegistrationDto?> GetOngoingRegistration(Guid userId, Guid employeeId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId != userId)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                employee = user;
            }

            var ongoingRegistration = await _dbContext.Registrations.FirstOrDefaultAsync(or =>
                or.Status == RegistrationStatus.Ongoing && or.EmployeeId == employee.Id && or.CorrectionOfId == null);

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

        public async Task<List<LogpunchRegistrationDto>> GetUnsettledWorkRegistrations(Guid userId, Guid employeeId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId != userId)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                employee = user;
            }

            List<LogpunchRegistration> registrations = _dbContext.Registrations.Where(r => r.EmployeeId == employeeId && r.Type == RegistrationType.Work && r.CorrectionOfId == null).Where(r => r.Status != RegistrationStatus.Ongoing && r.Status != RegistrationStatus.Settled).ToList();

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

        public async Task<OverviewResponse> WorkOverviewQuery(Guid userId, Guid employeeId, bool sortAsc, bool showUnitsWithNoRecords, bool setDefault,
                DateTimeOffset? customStartDate, DateTimeOffset? customEndDate, string timePeriod, string timeMode, string groupBy, string thenBy)
        {
            RegistrationType registrationType = RegistrationType.Work;
            DateTimeOffset startDate;
            DateTimeOffset endDate;
            string queryString;
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId != userId)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                employee = user;
            }

            if (customStartDate is null && customEndDate is null)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showUnitsWithNoRecords}, set_default={setDefault}, start_date=null, end_date=null, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }
            else if (customStartDate.HasValue && customEndDate.HasValue)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showUnitsWithNoRecords}, set_default={setDefault}, start_date={customStartDate.Value.DateTime.ToShortDateString()}, end_date={customEndDate.Value.DateTime.ToShortDateString()}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }
            else
            {
                throw new InvalidOperationException($"customStartDate and customEndDate must both either be null or have value. customStartDate: {customStartDate}, customEndDate: {customEndDate}");
            }

            if (timeMode != "custom" && (customEndDate.HasValue || customStartDate.HasValue) || timePeriod != "custom" && (customEndDate.HasValue || customStartDate.HasValue))
            {
                throw new InvalidOperationException("Invalid input combination: timeMode and timePeriod have to be 'custom' if you choose an endDate or endDate has to be left blank to see the chosen timeMode/timePeriod combination.");
            }

            if (customEndDate is not null && customStartDate is not null && customEndDate < customStartDate)
            {
                throw new InvalidOperationException("Invalid customEndDate: customEndDate can't be earlier than customStartDate");
            }

            if (timePeriod != "custom" && timeMode != "custom" && customStartDate is null && customEndDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(DateTimeOffset.Now, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(CalenderService.FindEndDate(startDate, timePeriod, timeMode));
            }
            else if (timePeriod == "custom" && timeMode == "custom" && customStartDate is not null && customEndDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(customStartDate.Value, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(DateTimeOffset.Now);
            }
            else
            {
                throw new InvalidOperationException("You cannot use custom dates if custom timePeriod and timeMode are not chosen");
            }

            Console.WriteLine($"Start date: {startDate} End date: {endDate}");

            if (!IsGroupByValid(groupBy, startDate, endDate))
            {
                throw new ArgumentException($"Invalid groupby option selected: A time period can only be grouped into period units that it contains more than one of. startDate: {startDate} endDate: {endDate}");
            }

            var groupByObjects = await GetGroupByObjects(groupBy, employee, startDate, endDate, showUnitsWithNoRecords, registrationType, thenBy, timePeriod);
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

            string timespan = $"{startDate.ToString(dateFormat, CultureInfo.InvariantCulture)} - {endDate.ToString(dateFormat, CultureInfo.InvariantCulture)}";

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

        public async Task<List<LogpunchRegistrationDto>> GetUnsettledTransportationRegistrations(Guid userId, Guid employeeId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId != userId)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                employee = user;
            }

            List<LogpunchRegistration> registrations = _dbContext.Registrations.Where(r => r.EmployeeId == employeeId && r.Type == RegistrationType.Transportation && r.CorrectionOfId == null).Where(r => r.Status != RegistrationStatus.Ongoing && r.Status != RegistrationStatus.Settled).ToList();

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

        public async Task<OverviewResponse> TransportationOverviewQuery(Guid userId, Guid employeeId, bool sortAsc, bool showUnitsWithNoRecords,
               DateTimeOffset? customStartDate, DateTimeOffset? customEndDate, string timePeriod, string timeMode, string groupBy, string thenBy)
        {
            RegistrationType registrationType = RegistrationType.Transportation;
            DateTimeOffset startDate;
            DateTimeOffset endDate;
            string queryString;
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId != userId)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                employee = user;
            }

            if (customStartDate is null && customEndDate is null)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showUnitsWithNoRecords}, start_date=null, end_date=null, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }
            else if (customStartDate.HasValue && customEndDate.HasValue)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showUnitsWithNoRecords}, start_date={customStartDate.Value.DateTime.ToShortDateString()}, end_date={customEndDate.Value.DateTime.ToShortDateString()}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }
            else
            {
                throw new InvalidOperationException($"customStartDate and customEndDate must both either be null or have value. customStartDate: {customStartDate}, customEndDate: {customEndDate}");
            }

            if (timeMode != "custom" && (customEndDate.HasValue || customStartDate.HasValue) || timePeriod != "custom" && (customEndDate.HasValue || customStartDate.HasValue))
            {
                throw new InvalidOperationException("Invalid input combination: timeMode and timePeriod have to be 'custom' if you choose an endDate or endDate has to be left blank to see the chosen timeMode/timePeriod combination.");
            }

            if (customEndDate is not null && customStartDate is not null && customEndDate < customStartDate)
            {
                throw new InvalidOperationException("Invalid customEndDate: customEndDate can't be earlier than customStartDate");
            }

            if (timePeriod != "custom" && timeMode != "custom" && customStartDate is null && customEndDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(DateTimeOffset.Now, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(CalenderService.FindEndDate(startDate, timePeriod, timeMode));
            }
            else if (timePeriod == "custom" && timeMode == "custom" && customStartDate is not null && customEndDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(customStartDate.Value, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(DateTimeOffset.Now);
            }
            else
            {
                throw new InvalidOperationException("You cannot use custom dates if custom timePeriod and timeMode are not chosen");
            }

            Console.WriteLine($"Start date: {startDate} End date: {endDate}");

            if (!IsGroupByValid(groupBy, startDate, endDate))
            {
                throw new ArgumentException($"Invalid groupby option selected: A time period can only be grouped into period units that it contains more than one of. startDate: {customStartDate} endDate: {endDate}");
            }

            var groupByObjects = await GetGroupByObjects(groupBy, employee, startDate, endDate, showUnitsWithNoRecords, registrationType, thenBy, timePeriod);
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

            string timespan = $"{startDate.ToString(dateFormat, CultureInfo.InvariantCulture)} - {endDate.ToString(dateFormat, CultureInfo.InvariantCulture)}";

            TimeModePeriodObject timeModePeriodObject = new(
                timeModePeriodName,
                timespan,
                total);

            TimePeriodObject timePeriodObject = new(groupByObjects);

            return new OverviewResponse(registrationType.ToString(), queryString, timeModePeriodObject, timePeriodObject);
        }

        // Absence

        public async Task<List<LogpunchRegistrationDto>> GetUnsettledAbsenceRegistrations(Guid userId, Guid employeeId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (userId == employeeId)
            {
                employee = user;
            }
            else
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No user with given ID exists");
            }

            List<LogpunchRegistration> registrations = _dbContext.Registrations.Where(r => r.EmployeeId == employeeId && r.Type != RegistrationType.Work && r.Type != RegistrationType.Transportation && r.CorrectionOfId == null).Where(r => r.Status != RegistrationStatus.Ongoing && r.Status != RegistrationStatus.Settled).ToList();

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

        public async Task<OverviewResponse> AbsenceOverviewQuery(Guid userId, Guid employeeId, bool sortAsc, bool showUnitsWithNoRecords,
                DateTimeOffset? customStartDate, DateTimeOffset? customEndDate, string timePeriod, string timeMode, string groupBy, string thenBy, string absenceType)
        {
            RegistrationType registrationType = RegistrationTypeConverter.ConvertStringToEnum(absenceType);

            if (registrationType == RegistrationType.Work || registrationType == RegistrationType.Transportation)
            {
                throw new InvalidOperationException($"{registrationType} is not among the absence types. These are '{RegistrationType.Leave}', '{RegistrationType.Sickness} and {RegistrationType.Vacation}");
            }

            DateTimeOffset startDate;
            DateTimeOffset endDate;
            string queryString;
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new InvalidOperationException("No user with given ID exists");
            LogpunchUser employee;

            if (employeeId != userId)
            {
                employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId) ?? throw new InvalidOperationException("No employee with given ID exists");
            }
            else
            {
                employee = user;
            }

            if (customStartDate is null && customEndDate is null)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showUnitsWithNoRecords}, start_date=null, end_date=null, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}, absence_type={absenceType}";
            }
            else if (customStartDate.HasValue && customEndDate.HasValue)
            {
                queryString = $"sort_asc={sortAsc}, show_days_no_records={showUnitsWithNoRecords}, start_date={customStartDate.Value.DateTime.ToShortDateString()}, end_date={customEndDate.Value.DateTime.ToShortDateString()}, time_period={timePeriod}, time_mode={timeMode}, groupby={groupBy}, thenby={thenBy}";
            }
            else
            {
                throw new InvalidOperationException($"customStartDate and customEndDate must both either be null or have value. customStartDate: {customStartDate}, customEndDate: {customEndDate}");
            }

            if (timeMode != "custom" && (customEndDate.HasValue || customStartDate.HasValue) || timePeriod != "custom" && (customEndDate.HasValue || customStartDate.HasValue))
            {
                throw new InvalidOperationException("Invalid input combination: timeMode and timePeriod have to be 'custom' if you choose an endDate or endDate has to be left blank to see the chosen timeMode/timePeriod combination.");
            }

            if (customEndDate is not null && customStartDate is not null && customEndDate < customStartDate)
            {
                throw new InvalidOperationException("Invalid customEndDate: customEndDate can't be earlier than customStartDate");
            }

            if (timePeriod != "custom" && timeMode != "custom" && customStartDate is null && customEndDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(DateTimeOffset.Now, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(CalenderService.FindEndDate(startDate, timePeriod, timeMode));
            }
            else if (timePeriod == "custom" && timeMode == "custom" && customStartDate is not null && customEndDate is null)
            {
                startDate = CalenderService.SetMinTimeOnDate(CalenderService.FindStartDateOfTimePeriod(customStartDate.Value, timePeriod, timeMode));
                endDate = CalenderService.SetMaxTimeOnDate(DateTimeOffset.Now);
            }
            else
            {
                throw new InvalidOperationException("You cannot use custom dates if custom timePeriod and timeMode are not chosen");
            }

            Console.WriteLine($"Start date: {startDate} End date: {endDate}");

            if (!IsGroupByValid(groupBy, startDate, endDate))
            {
                throw new ArgumentException($"Invalid groupby option selected: A time period can only be grouped into period units that it contains more than one of. startDate: {customStartDate} endDate: {customEndDate}");
            }

            var groupByObjects = await GetGroupByObjects(groupBy, employee, startDate, endDate, showUnitsWithNoRecords, registrationType, thenBy, timePeriod);
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

            string timespan = $"{startDate.ToString(dateFormat, CultureInfo.InvariantCulture)} - {endDate.ToString(dateFormat, CultureInfo.InvariantCulture)}";

            TimeModePeriodObject timeModePeriodObject = new(
                timeModePeriodName,
                timespan,
                total);

            TimePeriodObject timePeriodObject = new(groupByObjects);

            return new OverviewResponse(registrationType.ToString(), queryString, timeModePeriodObject, timePeriodObject);
        }

        // Helper methods

        private async Task<List<GroupByObject>> GetGroupByObjects(string groupBy, LogpunchUser user, DateTimeOffset startDate, DateTimeOffset endDate, bool showUnitsWithNoRecords, RegistrationType registrationType, string thenBy, string timePeriod)
        {
            if (CalenderService.TimeUnitOrder.Contains(groupBy) && CalenderService.TimeUnitOrder.Contains(timePeriod))
            {
                if (CalenderService.TimeUnitOrder.IndexOf(groupBy) >= CalenderService.TimeUnitOrder.IndexOf(timePeriod))
                {
                    // If groupBy is equal or larger than timePeriod, adjust groupBy to be one level smaller than timePeriod
                    int timePeriodIndex = CalenderService.TimeUnitOrder.IndexOf(timePeriod);
                    groupBy = timePeriodIndex > 0 ? CalenderService.TimeUnitOrder[timePeriodIndex - 1] : timePeriod;
                }
            }

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
                "day" => GroupByDay(finalData, showUnitsWithNoRecords, startDate, endDate, thenBy),
                "week" => GroupByWeek(finalData, showUnitsWithNoRecords, startDate, endDate, thenBy),
                "month" => GroupByMonth(finalData, showUnitsWithNoRecords, startDate, endDate, thenBy),
                "year" => GroupByYear(finalData, showUnitsWithNoRecords, startDate, endDate, thenBy),
                "client" => GroupByClient(finalData, showUnitsWithNoRecords, employeeClientRelationIds, startDate, endDate, thenBy),
                _ => throw new ArgumentException($"Invalid groupby option selected: {groupBy}")
            };

            return result;
        }

        private List<ThenByObject> GetThenByObjects(IEnumerable<LogpunchRegistrationDto> group, string thenBy, string groupBy)
        {
            // Check if thenBy is a valid time unit and is larger or equal to groupBy
            if (CalenderService.TimeUnitOrder.Contains(thenBy) && CalenderService.TimeUnitOrder.Contains(groupBy))
            {
                if (CalenderService.TimeUnitOrder.IndexOf(thenBy) >= CalenderService.TimeUnitOrder.IndexOf(groupBy))
                {
                    // If thenBy is equal or larger than groupBy, adjust thenBy to be one level smaller than groupBy
                    int groupByIndex = CalenderService.TimeUnitOrder.IndexOf(groupBy);
                    thenBy = groupByIndex > 0 ? CalenderService.TimeUnitOrder[groupByIndex - 1] : groupBy;
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

        private List<GroupByObject> GroupByDay(List<LogpunchRegistrationDto> correctedData, bool showUnitsWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => r.Start.Date)
                .Select(group => new GroupByObject(
                    group.Key.ToString("dd/MM/yyyy"),
                    group.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(group, thenBy, "day")
                ))
                .ToList();

            if (showUnitsWithNoRecords)
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

        private List<GroupByObject> GroupByWeek(List<LogpunchRegistrationDto> correctedData, bool showUnitsWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year, Week = CalenderService.GetDanishWeekNumber(r.Start) })
                .Select(g => new GroupByObject(
                    $"Week {g.Key.Week}, {g.Key.Year}",
                    g.Sum(item => item.Amount ?? 0),
                    GetThenByObjects(g, thenBy, "week")
                ))
                .ToList();

            if (showUnitsWithNoRecords)
            {
                var startWeek = CalenderService.GetDanishWeekNumber(startDate);
                var endWeek = CalenderService.GetDanishWeekNumber(endDate);
                var allWeeks = Enumerable.Range(0, endDate.Subtract(startDate).Days / 7 + 1)
                    .Select(offset => startDate.AddDays(offset * 7))
                    .Select(date => new { date.Year, Week = CalenderService.GetDanishWeekNumber(date) })
                    .Distinct()
                    .Select(week => $"Week {week.Week}, {week.Year}");

                var missingWeeks = allWeeks.Except(result.Select(r => r.Name));

                var missingGroupByObjects = missingWeeks.Select(week => new GroupByObject(week, 0, new List<ThenByObject>()));

                result.AddRange(missingGroupByObjects);
                result = result.OrderBy(r => DateTime.ParseExact(r.Name.Split(',')[1].Trim() + "-W" + r.Name.Split(' ')[1], "yyyy-Www", CultureInfo.InvariantCulture)).ToList();
            }

            return result;
        }

        private List<GroupByObject> GroupByMonth(List<LogpunchRegistrationDto> correctedData, bool showUnitsWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year, r.Start.Month })
                .Select(g => new GroupByObject(
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month) + $" {g.Key.Year}",
                    g.Sum(r => r.Amount ?? 0),
                    GetThenByObjects(g, thenBy, "month")
                ))
                .ToList();

            if (showUnitsWithNoRecords)
            {
                var allMonths = Enumerable.Range(0, ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1)
                    .Select(offset => startDate.AddMonths(offset))
                    .Select(date => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + $" {date.Year}");

                var missingMonths = allMonths.Except(result.Select(r => r.Name));

                var missingGroupByObjects = missingMonths.Select(month => new GroupByObject(month, 0, new List<ThenByObject>()));

                result.AddRange(missingGroupByObjects);
                result = result.OrderBy(r => DateTime.ParseExact(r.Name, "MMMM yyyy", CultureInfo.InvariantCulture)).ToList();
            }

            return result;
        }

        private List<GroupByObject> GroupByYear(List<LogpunchRegistrationDto> correctedData, bool showUnitsWithNoRecords, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var result = correctedData
                .GroupBy(r => new { r.Start.Year })
                .Select(g => new GroupByObject(
                    g.Key.Year.ToString(),
                    g.Sum(r => r.Amount ?? 0),
                    GetThenByObjects(g, thenBy, "year")
                ))
                .ToList();

            if (showUnitsWithNoRecords)
            {
                var allYears = Enumerable.Range(startDate.Year, endDate.Year - startDate.Year + 1)
                    .Select(year => year.ToString());

                var missingYears = allYears.Except(result.Select(r => r.Name));

                var missingGroupByObjects = missingYears.Select(year => new GroupByObject(year, 0, new List<ThenByObject>()));

                result.AddRange(missingGroupByObjects);
                result = result.OrderBy(r => int.Parse(r.Name)).ToList();
            }

            return result;
        }

        private List<GroupByObject> GroupByClient(List<LogpunchRegistrationDto> correctedData, bool showUnitsWithNoRecords, List<Guid> employeeClientRelationIds, DateTimeOffset startDate, DateTimeOffset endDate, string thenBy)
        {
            var groupedData = correctedData
                .GroupBy(r => r.ClientId)
                .Select(group => new
                {
                    ClientId = group.Key,
                    TotalAmount = group.Sum(item => item.Amount ?? 0),
                    Registrations = group
                })
                .ToList();

            var result = groupedData
                .Select(group => new GroupByObject(
                    GetClientName(group.ClientId),
                    group.TotalAmount,
                    GetThenByObjects(group.Registrations, thenBy, "client")
                ))
                .ToList();

            if (showUnitsWithNoRecords)
            {
                var allClients = _dbContext.Clients
                    .Select(c => c.Name ?? "No Client")
                    .ToList();

                var missingClients = allClients.Except(result.Select(r => r.Name));

                var missingGroupByObjects = missingClients.Select(client => new GroupByObject(client, 0, new List<ThenByObject>()));

                result.AddRange(missingGroupByObjects);
                result = result.OrderBy(r => r.Name).ToList();
            }

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
