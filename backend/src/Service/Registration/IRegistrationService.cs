using Domain;
using Shared;

namespace Infrastructure;

public interface IRegistrationService
{
    Task<LogpunchRegistrationDto> CreateRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? firstComment, string? secondComment);
    Task<LogpunchRegistrationDto> StartShiftRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment);
    Task<LogpunchRegistrationDto> EndShiftRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment);
    Task<LogpunchRegistrationDto> EmployeeConfirmationRegistration(Guid userId, Guid registrationId);
    Task<LogpunchRegistrationDto> EmployeeCorrectionRegistration(Guid userId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId);
    Task<LogpunchRegistrationDto> UpdateRegistrationStatus(Guid userId, Guid registrationId, string newStatus);
    Task<LogpunchRegistrationDto> ChangeRegistrationType(Guid userId, Guid registrationId, string newType);
    Task<LogpunchRegistrationDto> AdminCorrectionRegistration(Guid userId, Guid employeeId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId);
}
