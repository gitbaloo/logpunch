using Shared;


public interface IOverviewService
{
    Task<OverviewResponse> OverviewQuery(string token, bool sortAsc, bool showDaysWithNoRecords, bool setDefault, DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy);
    Task<string> GetDefaultQuery(string token);
}
