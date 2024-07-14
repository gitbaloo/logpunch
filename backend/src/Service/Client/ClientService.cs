using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Shared;

namespace Infrastructure.Client;

public class ClientService : IClientService
{
    private readonly LogpunchDbContext _dbContext;

    public ClientService(LogpunchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<LogpunchClientDto>> GetClients(Guid employeeId)
    {
        var employeeClientRelations = await _dbContext.EmployeeClientRelations
            .Where(ecr => ecr.EmployeeId == employeeId)
            .Include(ecr => ecr.Client)
            .ToListAsync();

        var clients = new List<LogpunchClientDto>();

        if (employeeClientRelations is null || !employeeClientRelations.Any())
        {
            Console.WriteLine("Employee has no relation to any clients");
            return clients;
        }

        foreach (var employeeClientRelation in employeeClientRelations)
        {
            if (employeeClientRelation.Client is not null)
            {
                LogpunchClientDto client = new LogpunchClientDto()
                {
                    Id = employeeClientRelation.ClientId,
                    Name = employeeClientRelation.Client.Name
                };

                clients.Add(client);
            }
            else
            {
                Console.WriteLine($"Client is null for relation with EmployeeId: {employeeClientRelation.EmployeeId}");
            }
        }

        return clients;
    }
}
