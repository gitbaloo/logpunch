using Domain;

namespace Infrastructure;

public interface IRegistrationService
{
    Task<LogpunchRegistration> EmployeeCreateRegistration(Guid employeeId, Guid clientId, int amount, string status, string? internalComment);
}
