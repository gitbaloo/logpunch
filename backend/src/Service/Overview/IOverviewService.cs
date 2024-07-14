using Shared;


public interface IOverviewService
{
    Task<LogpunchRegistrationDto> GetOngoingRegistration(Guid userId, Guid employeeId);
    Task<List<LogpunchRegistrationDto> GetUnsettledWorkRegistrations(Guid userId, Guid employeeId);
    Task<OverviewResponse> OverviewQuery(Guid userId, bool sortAsc, bool showDaysWithNoRecords, bool setDefault, DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy, string registrationTypeString);
    Task<string> GetDefaultQuery(Guid userId);
}
