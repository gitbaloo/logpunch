using Domain;
using Shared;

namespace Infrastructure.Client;

public interface IClientService
{
    Task<List<LogpunchClientDto>> GetClients(Guid employeeId);
}
