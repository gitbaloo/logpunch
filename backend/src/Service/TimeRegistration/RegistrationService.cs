using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure;

public class RegistrationService : IRegistrationService
{
    private readonly LogpunchDbContext _dbContext;

    public RegistrationService(LogpunchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LogpunchRegistration> CreateRegistration(Guid employeeId, Guid clientId, int amount, string status, string? internalComment, Guid? taskId)
    {
        LogpunchUser employee = _dbContext.Users.FirstOrDefault(c => c.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");


        Domain.LogpunchClient client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId)
            ?? throw new InvalidOperationException("Client not found");

        EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
            c.EmployeeId == employeeId
            && c.ClientId == clientId);

        if (employeeClientRelation is null)
        {
            employeeClientRelation = new EmployeeClientRelation(employee, client);
            _dbContext.EmployeeClientRelations.Add(employeeClientRelation);

            await _dbContext.SaveChangesAsync();
        }

        var registeredTime = new LogpunchRegistration(employee, );
        await _dbContext.Registrations.AddAsync(registeredTime);
        await _dbContext.SaveChangesAsync();

        return registeredTime;
    }
}
