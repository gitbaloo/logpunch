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
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var employeeId = userId;
        var clientId = Guid.NewGuid();
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee));
        context.Clients.Add(TestEntityFactory.CreateLogpunchClient(clientId, "Test Client"));
        context.EmployeeClientRelations.Add(new EmployeeClientRelation { EmployeeId = employeeId, ClientId = clientId });
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = DateTimeOffset.UtcNow;

        // Act
        var registration = await service.CreateWorkRegistration(userId, employeeId, clientId, startTime, endTime, "firstComment", "secondComment");

        // Assert
        Assert.NotNull(registration);
        Assert.Equal(userId, registration.CreatorId);
    }

    [Fact]
    public async Task CreateRegistration_InvalidTime_ThrowsException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var employeeId = userId;
        var clientId = Guid.NewGuid();
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee));
        context.Clients.Add(TestEntityFactory.CreateLogpunchClient(clientId, "Test Client"));
        context.EmployeeClientRelations.Add(new EmployeeClientRelation { EmployeeId = employeeId, ClientId = clientId });
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow;
        var endTime = DateTimeOffset.UtcNow.AddHours(-1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWorkRegistration(userId, employeeId, clientId, startTime, endTime, "firstComment", "secondComment"));
    }

    [Fact]
    public async Task StartShiftRegistration_ValidData_ReturnsRegistration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var employeeId = userId;
        var clientId = Guid.NewGuid();
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee));
        context.Clients.Add(TestEntityFactory.CreateLogpunchClient(clientId, "Test Client"));
        context.EmployeeClientRelations.Add(new EmployeeClientRelation { EmployeeId = employeeId, ClientId = clientId });
        await context.SaveChangesAsync();

        // Act
        var registration = await service.StartWorkRegistration(userId, employeeId, clientId, "firstComment");

        // Assert
        Assert.NotNull(registration);
        Assert.Equal(employeeId, registration.EmployeeId);
        Assert.Equal(clientId, registration.ClientId);
        Assert.Equal(RegistrationStatus.Ongoing, registration.Status);
    }

    [Fact]
    public async Task EndShiftRegistration_ValidData_ReturnsRegistration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var employeeId = userId;
        var registrationId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var registration = new LogpunchRegistration(employeeId, RegistrationType.Work, null, startTime, null, userId, clientId, startTime, RegistrationStatus.Ongoing, "firstComment", null, null);

        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var employee = user;

        context.Users.Add(user);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Detach entities to simulate independent instances
        context.Entry(user).State = EntityState.Detached;
        context.Entry(registration).State = EntityState.Detached;

        using var newContext = new LogpunchDbContext(options);
        var newService = new RegistrationService(newContext);

        // Act
        var result = await newService.EndWorkRegistration(userId, employeeId, registration.Id, "secondComment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RegistrationStatus.Open, result.Status);
        Assert.Equal("secondComment", result.SecondComment);
    }

    [Fact]
    public async Task UpdateRegistrationStatus_ValidData_ReturnsRegistration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var registration = new LogpunchRegistration(employeeId, RegistrationType.Work, 60, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, userId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Open, "firstComment", "secondComment", null);
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(userId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin));
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateRegistrationStatus(userId, registration.Id, RegistrationStatus.Settled.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RegistrationStatus.Settled, result.Status);
    }

    [Fact]
    public async Task ChangeRegistrationType_ValidData_ReturnsRegistration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var registration = new LogpunchRegistration(employeeId, RegistrationType.Work, 60, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, userId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Open, "firstComment", "secondComment", null);
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(userId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin));
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ChangeRegistrationType(userId, registration.Id, RegistrationType.Vacation.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RegistrationType.Vacation, result.Type);
    }

    [Fact]
    public async Task CreateCorrectionRegistration_ValidData_ReturnsRegistration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context);

        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var correctionOfId = Guid.NewGuid();
        var existingRegistration = new LogpunchRegistration(employeeId, RegistrationType.Work, 60, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), userId, clientId, DateTimeOffset.UtcNow.AddHours(-2), RegistrationStatus.Open, "initialComment", null, null);
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(userId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin));
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(employeeId, "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee));
        context.Clients.Add(TestEntityFactory.CreateLogpunchClient(clientId, "Test Client"));
        context.EmployeeClientRelations.Add(new EmployeeClientRelation { EmployeeId = employeeId, ClientId = clientId });
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var end = DateTimeOffset.UtcNow;

        // Act
        var result = await service.AdminCorrectionRegistration(userId, employeeId, start, end, clientId, "correctedFirstComment", "correctedSecondComment", existingRegistration.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingRegistration.Id, result.CorrectionOfId);
        Assert.Equal("correctedFirstComment", result.FirstComment);
        Assert.Equal("correctedSecondComment", result.SecondComment);
    }
}
