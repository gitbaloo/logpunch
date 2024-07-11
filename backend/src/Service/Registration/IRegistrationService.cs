using Domain;

namespace Infrastructure;

public interface IRegistrationService
{
    Task<LogpunchRegistration> CreateRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? firstComment, string? secondComment);
    Task<LogpunchRegistration> StartShiftRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment);
    Task<LogpunchRegistration> EndShiftRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment);
    Task<LogpunchRegistration> UpdateRegistrationStatus(Guid userId, Guid registrationId, int newStatus);
    Task<LogpunchRegistration> ChangeRegistrationType(Guid userId, Guid registrationId, int newType);
    Task<LogpunchRegistration> CreateCorrectionRegistration(Guid userId, Guid employeeId, int type, DateTimeOffset start, DateTimeOffset end, Guid? clientId, int status, string? firstComment, string? secondComment, Guid correctionOfId);
}
