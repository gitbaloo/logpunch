using Domain;

namespace Infrastructure;

public interface IRegistrationService
{
    Task<LogpunchRegistration> EmployeeCreateRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? internalComment);
    Task<LogpunchRegistration> StartShiftRegistration(Guid userId, Guid employeeId, Guid? clientId, string? internalComment);
    Task<LogpunchRegistration> EndShiftRegistration(Guid userId, Guid employeeId, Guid registrationId, string? internalComment);
}
