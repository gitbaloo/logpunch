using Domain;
using Shared;

namespace Infrastructure
{
    public interface IClientService
    {
        Task<List<LogpunchClientDto>> GetClients(Guid employeeId);
    }
}
