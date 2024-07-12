using System;
using System.Threading.Tasks;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;
using Moq;

public class RegistrationServiceTests
{
    [Fact]
    public async Task CreateRegistration_ValidData_ReturnsRegistration()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        context.Users.Add(new LogpunchUser { Id = userId });
        context.Users.Add(new LogpunchUser { Id = employeeId });
        context.Clients.Add(new LogpunchClient { Id = clientId });
        context.EmployeeClientRelations.Add(new EmployeeClientRelation { EmployeeId = employeeId, ClientId = clientId });
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = DateTimeOffset.UtcNow;

        var registration = await service.CreateRegistration(userId, employeeId, clientId, startTime, endTime, "firstComment", "secondComment");

        Assert.NotNull(registration);
        Assert.Equal(userId, registration.CreatorId);
    }

    [Fact]
    public async Task CreateRegistration_InvalidTime_ThrowsException()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        context.Users.Add(new LogpunchUser { Id = userId });
        context.Users.Add(new LogpunchUser { Id = employeeId });
        context.Clients.Add(new LogpunchClient { Id = clientId });
        context.EmployeeClientRelations.Add(new EmployeeClientRelation { EmployeeId = employeeId, ClientId = clientId });
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow;
        var endTime = DateTimeOffset.UtcNow.AddHours(-1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRegistration(userId, employeeId, clientId, startTime, endTime, "firstComment", "secondComment"));
    }
}
