using Domain;
using Shared;

namespace Infrastructure.Customer;

public interface ICustomerService
{
    Task<List<LogpunchClientDto>> GetCustomers(int consultantId);
}
