using Domain;
using Shared;

namespace Infrastructure.Customer;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetCustomers(int consultantId);
}
