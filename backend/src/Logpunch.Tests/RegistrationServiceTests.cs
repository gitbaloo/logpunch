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
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = DateTimeOffset.UtcNow;

        // Act
        var result = await service.CreateWorkRegistration(user.Id, user.Id, null, startTime, endTime, "First comment", "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.EmployeeId);
        Assert.Equal("Work", result.Type);
    }

    [Fact]
    public async Task CreateWorkRegistration_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow;
        var endTime = DateTimeOffset.UtcNow.AddHours(-1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateWorkRegistration(user.Id, user.Id, null, startTime, endTime, "First comment", "Second comment"));
    }


    [Fact]
    public async Task StartWorkRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());


        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.StartWorkRegistration(user.Id, user.Id, null, "First comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.EmployeeId);
        Assert.Equal("Work", result.Type);
        Assert.Equal("Ongoing", result.Status);
    }

    [Fact]
    public async Task StartWorkRegistration_OngoingRegistrationExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-2), null, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();


        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartWorkRegistration(user.Id, user.Id, null, "First comment"));
    }

    [Fact]
    public async Task EndWorkRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), null, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EndWorkRegistration(user.Id, user.Id, registration.Id, "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registration.Id, result.Id);
        Assert.Equal("Open", result.Status);
        Assert.Equal("Second comment", result.SecondComment);
    }

    [Fact]
    public async Task EndWorkRegistration_InvalidUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var otherUserId = Guid.NewGuid();

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), otherUserId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), null, otherUserId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndWorkRegistration(user.Id, otherUserId, registration.Id, "Second comment"));
    }

    [Fact]
    public async Task EndWorkRegistration_NonOngoingStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
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
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = DateTimeOffset.UtcNow;

        // Act
        var result = await service.CreateTransportationRegistration(user.Id, user.Id, client.Id, startTime, endTime, "First comment", "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.EmployeeId);
        Assert.Equal("Transportation", result.Type);
    }

    [Fact]
    public async Task CreateTransportationRegistration_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow;
        var endTime = DateTimeOffset.UtcNow.AddHours(-1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTransportationRegistration(user.Id, user.Id, client.Id, startTime, endTime, "First comment", "Second comment"));
    }

    [Fact]
    public async Task CreateTransportationRegistration_OverlappingRegistration_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), user.Id, client.Id, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        var startTime = DateTimeOffset.UtcNow.AddHours(-1.5);
        var endTime = DateTimeOffset.UtcNow;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateTransportationRegistration(user.Id, user.Id, client.Id, startTime, endTime, "First comment", "Second comment"));
    }

    [Fact]
    public async Task StartTransportationRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));
        await context.SaveChangesAsync();

        // Act
        var result = await service.StartTransportationRegistration(user.Id, user.Id, client.Id, "First comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.EmployeeId);
        Assert.Equal("Transportation", result.Type);
        Assert.Equal("Ongoing", result.Status);
    }

    [Fact]
    public async Task StartTransportationRegistration_OngoingRegistrationExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "ClientName");
        context.Users.Add(user);
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), user, client));

        var existingRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-2), null, user.Id, client.Id, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, null, null, null);
        context.Registrations.Add(existingRegistration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartTransportationRegistration(user.Id, user.Id, client.Id, "First comment"));
    }

    [Fact]
    public async Task EndTransportationRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-1), null, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EndTransportationRegistration(user.Id, user.Id, registration.Id, "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registration.Id, result.Id);
        Assert.Equal("Open", result.Status);
        Assert.Equal("Second comment", result.SecondComment);
    }

    [Fact]
    public async Task EndTransportationRegistration_InvalidUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var otherUserId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), otherUserId, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-1), null, otherUserId, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndTransportationRegistration(user.Id, otherUserId, registration.Id, "Second comment"));
    }

    [Fact]
    public async Task EndTransportationRegistration_NonOngoingStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EndTransportationRegistration(user.Id, user.Id, registration.Id, "Second comment"));
    }

    [Fact]
    public async Task EmployeeConfirmationRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EmployeeConfirmationRegistration(user.Id, registration.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Awaiting", result.Status);
    }

    [Fact]
    public async Task EmployeeConfirmationRegistration_InvalidUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var otherUserId = Guid.NewGuid();
        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), otherUserId, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, otherUserId, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EmployeeConfirmationRegistration(user.Id, registration.Id));
    }

    [Fact]
    public async Task EmployeeConfirmationRegistration_NonOpenStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EmployeeConfirmationRegistration(user.Id, registration.Id));
    }

    [Fact]
    public async Task EmployeeCorrectionRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EmployeeCorrectionRegistration(user.Id, null, "Work", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, "Correction comment", null, originalRegistration.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Work", result.Type);
        Assert.Equal("Correction comment", result.FirstComment);
        Assert.Equal(originalRegistration.Id, result.CorrectionOfId);
    }

    [Fact]
    public async Task EmployeeCorrectionRegistration_InvalidType_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Leave, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EmployeeCorrectionRegistration(user.Id, null, "Leave", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, "Correction comment", null, originalRegistration.Id));
    }


    [Fact]
    public async Task CreateAbsenceRegistration_ValidInput_ReturnsRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var calendarServiceMock = new Mock<ICalendarService>();
        calendarServiceMock.Setup(x => x.IsDateValid(It.IsAny<DateTimeOffset>())).ReturnsAsync(true);
        calendarServiceMock.Setup(x => x.HolidaysAndWeekendDatesInTimeSpan(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        var service = new RegistrationService(context, calendarServiceMock.Object);

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CreateAbsenceRegistration(admin.Id, employee.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1), "Sickness", "First comment", "Second comment");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sickness", result.Type);
        Assert.Equal(employee.Id, result.EmployeeId);
        Assert.Equal(admin.Id, result.CreatorId);
    }

    [Fact]
    public async Task CreateAbsenceRegistration_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var calendarServiceMock = new Mock<ICalendarService>();
        calendarServiceMock.Setup(x => x.IsDateValid(It.IsAny<DateTimeOffset>())).ReturnsAsync(true);
        calendarServiceMock.Setup(x => x.HolidaysAndWeekendDatesInTimeSpan(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        var service = new RegistrationService(context, calendarServiceMock.Object);

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAbsenceRegistration(admin.Id, employee.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1), "Vacation", "First comment", "Second comment"));
    }

    [Fact]
    public async Task CreateAbsenceRegistration_NonAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var calendarServiceMock = new Mock<ICalendarService>();
        calendarServiceMock.Setup(x => x.IsDateValid(It.IsAny<DateTimeOffset>())).ReturnsAsync(true);
        calendarServiceMock.Setup(x => x.HolidaysAndWeekendDatesInTimeSpan(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        var service = new RegistrationService(context, calendarServiceMock.Object);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAbsenceRegistration(user.Id, employee.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1), "Sickness", "First comment", "Second comment"));
    }

    [Fact]
    public async Task UpdateRegistrationStatus_ValidInput_ReturnsUpdatedRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), admin.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), admin.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateRegistrationStatus(admin.Id, registration.Id, "Approved");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);
    }

    [Fact]
    public async Task UpdateRegistrationStatus_NonAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateRegistrationStatus(user.Id, registration.Id, "Approved"));
    }

    [Fact]
    public async Task UpdateRegistrationStatus_InvalidStatus_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), admin.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), admin.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateRegistrationStatus(admin.Id, registration.Id, "InvalidStatus"));
    }

    [Fact]
    public async Task ChangeRegistrationType_ValidInput_ReturnsUpdatedRegistrationDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), admin.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), admin.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ChangeRegistrationType(admin.Id, registration.Id, "Leave");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Leave", result.Type);
    }

    [Fact]
    public async Task ChangeRegistrationType_NonAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangeRegistrationType(user.Id, registration.Id, "Leave"));
    }

    [Fact]
    public async Task ChangeRegistrationType_InvalidType_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "Employee", "User", null, UserRole.Employee);
        context.Users.Add(employee);

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), employee.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), admin.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ChangeRegistrationType(admin.Id, registration.Id, "InvalidType"));
    }

    [Fact]
    public async Task AdminCorrectionRegistration_ValidInput_ReturnsCorrectionDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);

        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "ClientName");
        context.Clients.Add(client);
        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), employee, client));

        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), employee.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), admin.Id, client.Id, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.AdminCorrectionRegistration(admin.Id, employee.Id, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, client.Id, "Correction comment", null, originalRegistration.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Correction comment", result.FirstComment);
        Assert.Equal(originalRegistration.Id, result.CorrectionOfId);
    }

    [Fact]
    public async Task AdminCorrectionRegistration_InvalidEmployee_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);

        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), employee.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), admin.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);
        await context.SaveChangesAsync();

        var invalidEmployeeId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AdminCorrectionRegistration(admin.Id, invalidEmployeeId, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, null, "Correction comment", null, originalRegistration.Id));
    }

    [Fact]
    public async Task AdminCorrectionRegistration_CorrectionOfCorrection_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new RegistrationService(context, Mock.Of<ICalendarService>());

        var admin = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "admin@example.com", "password", "Admin", "User", null, UserRole.Admin);
        context.Users.Add(admin);

        var employee = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "employee@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(employee);

        var client = TestEntityFactory.CreateLogpunchClient(Guid.NewGuid(), "ClientName");
        context.Clients.Add(client);

        context.EmployeeClientRelations.Add(TestEntityFactory.CreateEmployeeClientRelation(Guid.NewGuid(), employee, client));

        var originalRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), employee.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), admin.Id, client.Id, DateTimeOffset.UtcNow, RegistrationStatus.Open, "First comment", null, null);
        context.Registrations.Add(originalRegistration);

        var correctionRegistration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), employee.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, admin.Id, client.Id, DateTimeOffset.UtcNow, RegistrationStatus.Open, "Correction comment", null, originalRegistration.Id);
        context.Registrations.Add(correctionRegistration);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AdminCorrectionRegistration(admin.Id, employee.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1), client.Id, "Second Correction", null, correctionRegistration.Id));
    }

}
