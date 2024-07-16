using Domain;
using Shared;

namespace Infrastructure
{
    public interface IRegistrationService
    {
        Task<LogpunchRegistrationDto> CreateWorkRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? firstComment, string? secondComment);
        Task<LogpunchRegistrationDto> StartWorkRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment);
        Task<LogpunchRegistrationDto> EndWorkRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment);
        Task<LogpunchRegistrationDto> CreateTransportationRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? firstComment, string? secondComment);
        Task<LogpunchRegistrationDto> StartTransportationRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment);
        Task<LogpunchRegistrationDto> EndTransportationRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment);
        Task<LogpunchRegistrationDto> EmployeeConfirmationRegistration(Guid userId, Guid registrationId);
        Task<LogpunchRegistrationDto> EmployeeCorrectionRegistration(Guid userId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId);
        Task<LogpunchRegistrationDto> CreateAbsenceRegistration(Guid userId, Guid employeeId, DateTimeOffset start, DateTimeOffset end, string type, string? firstComment, string? secondComment);
        Task<LogpunchRegistrationDto> UpdateRegistrationStatus(Guid userId, Guid registrationId, string newStatus);
        Task<LogpunchRegistrationDto> ChangeRegistrationType(Guid userId, Guid registrationId, string newType);
        Task<LogpunchRegistrationDto> AdminCorrectionRegistration(Guid userId, Guid employeeId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId);
    }
}
