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
    public async Task CreateWorkRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb25")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var employeeId = userId;
        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = DateTimeOffset.UtcNow;

        // Act
        var result = await service.CreateWorkRegistration(userId, employeeId, null, startTime, endTime, "First comment", "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal("Work", result.Type);
    }

    [Fact]
    public async Task CreateWorkRegistration_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb26")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var employeeId = userId;
        var startTime = DateTimeOffset.UtcNow;
        var endTime = DateTimeOffset.UtcNow.AddHours(-1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWorkRegistration(userId, employeeId, null, startTime, endTime, "First comment", "Second comment"));
    }

    [Fact]
    public async Task CreateWorkRegistration_OverlappingRegistration_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb27")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var employeeId = userId;
        var startTime = DateTimeOffset.UtcNow.AddHours(-1.5);
        var endTime = DateTimeOffset.UtcNow;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWorkRegistration(userId, employeeId, null, startTime, endTime, "First comment", "Second comment"));
    }

    [Fact]
    public async Task StartWorkRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb28")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var employeeId = userId;

        // Act
        var result = await service.StartWorkRegistration(userId, employeeId, null, "First comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal("Work", result.Type);
        Assert.Equal("Ongoing", result.Status);
    }

    [Fact]
    public async Task StartWorkRegistration_OngoingRegistrationExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb29")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-2), null, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var employeeId = userId;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartWorkRegistration(userId, employeeId, null, "First comment"));
    }

    [Fact]
    public async Task StartWorkRegistration_OverlappingRegistration_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb30")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var employeeId = userId;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartWorkRegistration(userId, employeeId, null, "First comment"));
    }

    [Fact]
    public async Task EndWorkRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb31")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), null, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EndWorkRegistration(userId, userId, registrationId, "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registrationId, result.Id);
        Assert.Equal("Open", result.Status);
        Assert.Equal("Second comment", result.SecondComment);
    }

    [Fact]
    public async Task EndWorkRegistration_InvalidUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb32")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var otherUserId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, otherUserId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), null, otherUserId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndWorkRegistration(userId, otherUserId, registrationId, "Second comment"));
    }

    [Fact]
    public async Task EndWorkRegistration_NonOngoingStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb33")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndWorkRegistration(userId, userId, registrationId, "Second comment"));
    }

    [Fact]
    public async Task CreateTransportationRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb34")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));
        await context.SaveChangesAsync();

        var employeeId = userId;
        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = DateTimeOffset.UtcNow;

        // Act
        var result = await service.CreateTransportationRegistration(userId, employeeId, clientId, startTime, endTime, "First comment", "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal("Transportation", result.Type);
    }

    [Fact]
    public async Task CreateTransportationRegistration_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb35")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));
        await context.SaveChangesAsync();

        var employeeId = userId;
        var startTime = DateTimeOffset.UtcNow;
        var endTime = DateTimeOffset.UtcNow.AddHours(-1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTransportationRegistration(userId, employeeId, clientId, startTime, endTime, "First comment", "Second comment"));
    }

    [Fact]
    public async Task CreateTransportationRegistration_OverlappingRegistration_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb36")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), userId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), userId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var employeeId = userId;
        var startTime = DateTimeOffset.UtcNow.AddHours(-1.5);
        var endTime = DateTimeOffset.UtcNow;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTransportationRegistration(userId, employeeId, clientId, startTime, endTime, "First comment", "Second comment"));
    }

    [Fact]
    public async Task StartTransportationRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb37")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));
        await context.SaveChangesAsync();

        var employeeId = userId;

        // Act
        var result = await service.StartTransportationRegistration(userId, employeeId, clientId, "First comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal("Transportation", result.Type);
        Assert.Equal("Ongoing", result.Status);
    }

    [Fact]
    public async Task StartTransportationRegistration_OngoingRegistrationExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb38")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), userId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-2), null, userId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var employeeId = userId;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartTransportationRegistration(userId, employeeId, clientId, "First comment"));
    }

    [Fact]
    public async Task StartTransportationRegistration_OverlappingRegistration_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb39")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), userId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), userId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var employeeId = userId;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartTransportationRegistration(userId, employeeId, clientId, "First comment"));
    }

    [Fact]
    public async Task EndTransportationRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb40")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-1), null, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EndTransportationRegistration(userId, userId, registrationId, "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registrationId, result.Id);
        Assert.Equal("Open", result.Status);
        Assert.Equal("Second comment", result.SecondComment);
    }

    [Fact]
    public async Task EndTransportationRegistration_InvalidUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb41")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var otherUserId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, otherUserId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-1), null, otherUserId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndTransportationRegistration(userId, otherUserId, registrationId, "Second comment"));
    }

    [Fact]
    public async Task EndTransportationRegistration_NonOngoingStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb42")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndTransportationRegistration(userId, userId, registrationId, "Second comment"));
    }

    [Fact]
    public async Task EmployeeConfirmationRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb43")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EmployeeConfirmationRegistration(userId, registrationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Awaiting", result.Status);
    }

    [Fact]
    public async Task EmployeeConfirmationRegistration_InvalidUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb44")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var otherUserId = Guid.NewGuid();
        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, otherUserId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, otherUserId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EmployeeConfirmationRegistration(userId, registrationId));
    }

    [Fact]
    public async Task EmployeeConfirmationRegistration_NonOpenStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb45")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EmployeeConfirmationRegistration(userId, registrationId));
    }

    [Fact]
    public async Task EmployeeCorrectionRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb46")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var originalRegistrationId = Guid.NewGuid();
        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(originalRegistrationId, userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EmployeeCorrectionRegistration(userId, null, "Work", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, "Correction comment", null, originalRegistrationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Work", result.Type);
        Assert.Equal("Correction comment", result.FirstComment);
        Assert.Equal(originalRegistrationId, result.CorrectionOfId);
    }

    [Fact]
    public async Task EmployeeCorrectionRegistration_InvalidType_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb47")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var originalRegistrationId = Guid.NewGuid();
        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(originalRegistrationId, userId, RegistrationType.Leave, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EmployeeCorrectionRegistration(userId, null, "Leave", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, "Correction comment", null, originalRegistrationId));
    }

    [Fact]
    public async Task EmployeeCorrectionRegistration_OverlappingRegistration_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb48")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var originalRegistrationId = Guid.NewGuid();
        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(originalRegistrationId, userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);

        var overlappingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-1).AddHours(-1), DateTimeOffset.UtcNow, userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(overlappingRegistration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EmployeeCorrectionRegistration(userId, null, "Work", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, "Correction comment", null, originalRegistrationId));
    }

    [Fact]
    public async Task CreateAbsenceRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb49")
            .Options;

        using var context = new LogpunchDbContext(options);
        var calendarServiceMock = new Mock<ICalendarService>();
        calendarServiceMock.Setup(x => x.IsDateValid(It.IsAny<DateTimeOffset>())).ReturnsAsync(true);
        calendarServiceMock.Setup(x => x.HolidaysAndWeekendDatesInTimeSpan(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        var service = new RegistrationService(context, calendarServiceMock.Object);

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var employeeId = Guid.NewGuid();
        var employee = TestEntityFactory.CreateLogpunchUser(employeeId, "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CreateAbsenceRegistration(userId, employeeId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1), "Sickness", "First comment", "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sickness", result.Type);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(userId, result.CreatorId);
    }

    [Fact]
    public async Task CreateAbsenceRegistration_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb50")
            .Options;

        using var context = new LogpunchDbContext(options);
        var calendarServiceMock = new Mock<ICalendarService>();
        calendarServiceMock.Setup(x => x.IsDateValid(It.IsAny<DateTimeOffset>())).ReturnsAsync(true);
        calendarServiceMock.Setup(x => x.HolidaysAndWeekendDatesInTimeSpan(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        var service = new RegistrationService(context, calendarServiceMock.Object);

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var employeeId = Guid.NewGuid();
        var employee = TestEntityFactory.CreateLogpunchUser(employeeId, "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAbsenceRegistration(userId, employeeId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1), "Vacation", "First comment", "Second comment"));
    }

    [Fact]
    public async Task CreateAbsenceRegistration_NonAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb51")
            .Options;

        using var context = new LogpunchDbContext(options);
        var calendarServiceMock = new Mock<ICalendarService>();
        calendarServiceMock.Setup(x => x.IsDateValid(It.IsAny<DateTimeOffset>())).ReturnsAsync(true);
        calendarServiceMock.Setup(x => x.HolidaysAndWeekendDatesInTimeSpan(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        var service = new RegistrationService(context, calendarServiceMock.Object);

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var employeeId = Guid.NewGuid();
        var employee = TestEntityFactory.CreateLogpunchUser(employeeId, "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAbsenceRegistration(userId, employeeId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1), "Sickness", "First comment", "Second comment"));
    }

    [Fact]
    public async Task UpdateRegistrationStatus_ValidInput_ReturnsUpdatedRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb52")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var adminId = Guid.NewGuid();
        var admin = TestEntityFactory.CreateLogpunchUser(adminId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, adminId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), adminId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateRegistrationStatus(adminId, registrationId, "Approved");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
    }

    [Fact]
    public async Task UpdateRegistrationStatus_NonAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb53")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateRegistrationStatus(userId, registrationId, "Approved"));
    }

    [Fact]
    public async Task UpdateRegistrationStatus_InvalidStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb54")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var adminId = Guid.NewGuid();
        var admin = TestEntityFactory.CreateLogpunchUser(adminId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, adminId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), adminId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateRegistrationStatus(adminId, registrationId, "InvalidStatus"));
    }

    [Fact]
    public async Task ChangeRegistrationType_ValidInput_ReturnsUpdatedRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb55")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var adminId = Guid.NewGuid();
        var admin = TestEntityFactory.CreateLogpunchUser(adminId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, adminId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), adminId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ChangeRegistrationType(adminId, registrationId, "Leave");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Leave", result.Type);
    }

    [Fact]
    public async Task ChangeRegistrationType_NonAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb56")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var userId = Guid.NewGuid();
        var user = TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, userId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), userId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeRegistrationType(userId, registrationId, "Leave"));
    }

    [Fact]
    public async Task ChangeRegistrationType_InvalidType_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb57")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var adminId = Guid.NewGuid();
        var admin = TestEntityFactory.CreateLogpunchUser(adminId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registrationId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(registrationId, adminId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), adminId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeRegistrationType(adminId, registrationId, "InvalidType"));
    }

    [Fact]
    public async Task AdminCorrectionRegistration_ValidInput_ReturnsCorrectionDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb58")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var adminId = Guid.NewGuid();
        var admin = TestEntityFactory.CreateLogpunchUser(adminId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var employeeId = Guid.NewGuid();
        var employee = TestEntityFactory.CreateLogpunchUser(employeeId, "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);

        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), employee, client));

        var originalRegistrationId = Guid.NewGuid();
        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(originalRegistrationId, employeeId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), adminId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.AdminCorrectionRegistration(adminId, employeeId, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, clientId, "Correction comment", null, originalRegistrationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Correction comment", result.FirstComment);
        Assert.Equal(originalRegistrationId, result.CorrectionOfId);
    }

    [Fact]
    public async Task AdminCorrectionRegistration_InvalidEmployee_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb59")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var adminId = Guid.NewGuid();
        var admin = TestEntityFactory.CreateLogpunchUser(adminId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var employeeId = Guid.NewGuid();
        var employee = TestEntityFactory.CreateLogpunchUser(employeeId, "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);

        var originalRegistrationId = Guid.NewGuid();
        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(originalRegistrationId, employeeId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), adminId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        var invalidEmployeeId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AdminCorrectionRegistration(adminId, invalidEmployeeId, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, null, "Correction comment", null, originalRegistrationId));
    }

    [Fact]
    public async Task AdminCorrectionRegistration_CorrectionOfCorrection_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb60")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var adminId = Guid.NewGuid();
        var admin = TestEntityFactory.CreateLogpunchUser(adminId, "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var employeeId = Guid.NewGuid();
        var employee = TestEntityFactory.CreateLogpunchUser(employeeId, "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);

        var clientId = Guid.NewGuid();
        var client = TestEntityFactory.CreateLogpunchClient(clientId, "ClientName");
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), employee, client));

        var originalRegistrationId = Guid.NewGuid();
        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(originalRegistrationId, employeeId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), adminId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);

        var correctionRegistrationId = Guid.NewGuid();
        var correctionRegistration = TestEntityFactory.CreateLogpunchRegistration(correctionRegistrationId, employeeId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, adminId, clientId, DateTimeOffset.UtcNow, RegistrationStatus.Open, "Correction comment", null, originalRegistrationId);
        context.Registrations.Add(correctionRegistration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AdminCorrectionRegistration(adminId, employeeId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1), clientId, "Second Correction", null, correctionRegistrationId));
    }

}
