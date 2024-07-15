using Shared;

namespace Infrastructure
{
    public interface IOverviewService
    {
        Task<LogpunchRegistrationDto?> GetOngoingRegistration(Guid userId, Guid? employeeId);
        Task<List<LogpunchRegistrationDto>> GetUnsettledWorkRegistrations(Guid userId, Guid? employeeId);
        Task<OverviewResponse> WorkOverviewQuery(Guid userId, Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords, bool setDefault, DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy);
        Task<string> GetDefaultWorkQuery(Guid userId);
        Task<List<LogpunchRegistrationDto>> GetUnsettledAbsenceRegistrations(Guid userId, Guid? employeeId);
        Task<List<LogpunchRegistrationDto>> GetUnsettledTransportationRegistrations(Guid userId, Guid? employeeId);
        Task<OverviewResponse> AbsenceOverviewQuery(Guid userId, Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy, string absenceType);
        Task<OverviewResponse> TransportationOverviewQuery(Guid userId, Guid? employeeId, bool sortAsc, bool showDaysWithNoRecords, DateTimeOffset startDate, DateTimeOffset? endDate, string timePeriod, string timeMode, string groupBy, string thenBy);
    }
}
