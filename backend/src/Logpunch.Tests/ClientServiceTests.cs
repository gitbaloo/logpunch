using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Client;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Shared;
using Domain;
using Xunit;

public class ClientServiceTests
{
    [Fact]
    public async Task GetClients_ReturnsClientsForEmployee()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new ClientService(context);

        var employeeId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        context.EmployeeClientRelations.Add(new EmployeeClientRelation
        {
            EmployeeId = employeeId,
            ClientId = clientId,
            Client = new LogpunchClient { Id = clientId, Name = "Test Client" }
        });
        await context.SaveChangesAsync();

        var clients = await service.GetClients(employeeId);

        Assert.Single(clients);
        Assert.Equal("Test Client", clients[0].Name);
    }

    [Fact]
    public async Task GetClients_EmployeeHasNoClients_ReturnsEmptyList()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new ClientService(context);

        var employeeId = Guid.NewGuid();

        var clients = await service.GetClients(employeeId);

        Assert.Empty(clients);
    }
}
