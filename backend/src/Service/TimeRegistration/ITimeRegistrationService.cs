using Domain;

namespace Infrastructure;

public interface ITimeRegistrationService
{
    Task<TimeRegistration> RegisterTime(int consultantId, int customerId, double hours);
}
