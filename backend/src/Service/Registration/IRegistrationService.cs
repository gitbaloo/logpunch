using Domain;

namespace Infrastructure;

public interface IRegistrationService
{
    Task<LogpunchRegistration> CreateRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? firstComment, string? secondComment);
    Task<LogpunchRegistration> StartShiftRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment);
    Task<LogpunchRegistration> EndShiftRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment);
    Task<LogpunchRegistration> EmployeeConfirmationRegistration(Guid userId, Guid registrationId);
    Task<LogpunchRegistration> EmployeeCorrectionRegistration(Guid userId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId);
    Task<LogpunchRegistration> UpdateRegistrationStatus(Guid userId, Guid registrationId, string newStatus);
    Task<LogpunchRegistration> ChangeRegistrationType(Guid userId, Guid registrationId, string newType);
    Task<LogpunchRegistration> AdminCorrectionRegistration(Guid userId, Guid employeeId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId);
}
