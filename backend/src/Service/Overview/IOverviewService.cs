using Shared;


public interface IOverviewService
{
    Task<OverviewResponse> OverviewQuery(Guid userId, bool sortAsc, bool showDaysWithNoRecords, bool setDefault, DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy, string registrationTypeString);
    Task<string> GetDefaultQuery(Guid userId);
}
