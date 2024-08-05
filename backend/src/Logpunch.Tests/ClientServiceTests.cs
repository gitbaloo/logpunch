using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
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
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new ClientService(context);

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "Employee", "User", null, UserRole.Employee);
        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "Test Client");
        context.Users.Add(employee);
        context.Clients.Add(client);

        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), employee, client));
        await context.SaveChangesAsync();

        // Act
        var clients = await service.GetClients(employee.Id);

        // Assert
        Assert.Single(clients);
        Assert.Equal("Test Client", clients[0].Name);
    }

    [Fact]
    public async Task GetClients_EmployeeHasNoClients_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new ClientService(context);

        var employeeId = Guid.NewGuid();

        // Act
        var clients = await service.GetClients(employeeId);

        // Assert
        Assert.Empty(clients);
    }
}
