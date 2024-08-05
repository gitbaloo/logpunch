using System;
using System.Threading.Tasks;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Shared;
using Xunit;

public class OverviewServiceTests
{
    [Fact]
    public async Task GetOngoingRegistration_UserWithNoOngoingRegistration_ReturnsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetOngoingRegistration(user.Id, user.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOngoingRegistration_UserWithOngoingRegistration_ReturnsDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow, null, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Ongoing, null, null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetOngoingRegistration(user.Id, user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registration.Id, result?.Id);
    }

    [Fact]
    public async Task GetOngoingRegistration_NoEmployeeWithId_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var nonExistentEmployeeId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetOngoingRegistration(user.Id, nonExistentEmployeeId));
    }

    [Fact]
    public async Task GetUnsettledWorkRegistrations_UserWithNoRegistrations_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUnsettledWorkRegistrations(user.Id, user.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUnsettledWorkRegistrations_UserWithUnsettledRegistrations_ReturnsList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Work, null, DateTimeOffset.UtcNow, null, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUnsettledWorkRegistrations(user.Id, user.Id);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal(registration.Id, result.First().Id);
    }

    [Fact]
    public async Task GetUnsettledWorkRegistrations_ThrowsInvalidOperationExceptionForNonexistentEmployee()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var nonExistentEmployeeId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUnsettledWorkRegistrations(user.Id, nonExistentEmployeeId));
    }

    [Fact]
    public async Task WorkOverviewQuery_ValidParameters_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var response = await service.WorkOverviewQuery(user.Id, user.Id, true, false, false, null, null, "day", "current", "day", "none");

        // Assert
        Assert.NotNull(response);
        Assert.Contains("timePeriod=day", response.QueryString);
    }

    [Fact]
    public async Task WorkOverviewQuery_CustomDates_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow.AddDays(-7);
        var endDate = DateTimeOffset.UtcNow;

        // Act
        var response = await service.WorkOverviewQuery(user.Id, user.Id, true, false, false, startDate, endDate, "custom", "custom", "day", "none");

        // Assert
        Assert.NotNull(response);
        Assert.Contains($"startDate={startDate.DateTime.ToShortDateString()}", response.QueryString);
    }

    [Fact]
    public async Task WorkOverviewQuery_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow;
        var endDate = DateTimeOffset.UtcNow.AddDays(-7);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.WorkOverviewQuery(user.Id, user.Id, true, false, false, startDate, endDate, "custom", "custom", "day", "none"));
    }

    [Fact]
    public async Task WorkOverviewQuery_ValidData_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        // Act
        var response = await service.WorkOverviewQuery(user.Id, user.Id, true, false, false, startDate, endDate, "custom", "custom", "day", "none");

        // Assert
        Assert.NotNull(response);
        Assert.Contains("sortAsc=True", response.QueryString);
    }

    [Fact]
    public async Task GetDefaultWorkQuery_UserWithDefaultQuery_ReturnsDefaultQueryString()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var queryString = "sortAsc=False&showUnitsWithNoRecords=False&setDefault=False&timePeriod=year&timeMode=rolling&groupBy=client&thenBy=day";
        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", queryString, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDefaultWorkQuery(user.Id);

        // Assert
        Assert.Equal(queryString, result);
    }

    [Fact]
    public async Task GetDefaultWorkQuery_UserWithoutDefaultQuery_ReturnsDefaultString()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDefaultWorkQuery(user.Id);

        // Assert
        Assert.Equal("?sort_asc=false&show_days_no_records=false&set_default=false&start_date=null&end_date=null&time_period=week&time_mode=current&group_by=day&then_by=none", result);
    }

    [Fact]
    public async Task GetDefaultWorkQuery_InvalidUserId_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetDefaultWorkQuery(userId));
    }

    [Fact]
    public async Task GetUnsettledTransportationRegistrations_UserWithNoRegistrations_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUnsettledTransportationRegistrations(user.Id, user.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUnsettledTransportationRegistrations_UserWithUnsettledTransportationRegistrations_ReturnsList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Transportation, null, DateTimeOffset.UtcNow, null, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Open, null, null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUnsettledTransportationRegistrations(user.Id, user.Id);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal(registration.Id, result.First().Id);
    }

    [Fact]
    public async Task GetUnsettledTransportationRegistrations_ThrowsInvalidOperationExceptionForNonexistentEmployee()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var nonExistentEmployeeId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUnsettledTransportationRegistrations(user.Id, nonExistentEmployeeId));
    }

    [Fact]
    public async Task TransportationOverviewQuery_ValidParameters_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var response = await service.TransportationOverviewQuery(user.Id, user.Id, true, false, null, null, "week", "current", "day", "none");

        // Assert
        Assert.NotNull(response);
        Assert.Contains("timePeriod=week", response.QueryString);
    }

    [Fact]
    public async Task TransportationOverviewQuery_CustomDates_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow.AddDays(-7);
        var endDate = DateTimeOffset.UtcNow;

        // Act
        var response = await service.TransportationOverviewQuery(user.Id, user.Id, true, false, startDate, endDate, "custom", "custom", "day", "none");

        // Assert
        Assert.NotNull(response);
        Assert.Contains($"startDate={startDate.DateTime.ToShortDateString()}", response.QueryString);
    }

    [Fact]
    public async Task TransportationOverviewQuery_InvalidDateRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow;
        var endDate = DateTimeOffset.UtcNow.AddDays(-7);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransportationOverviewQuery(user.Id, user.Id, true, false, startDate, endDate, "custom", "custom", "day", "none"));
    }


    [Fact]
    public async Task GetUnsettledAbsenceRegistrations_UserWithNoRegistrations_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUnsettledAbsenceRegistrations(user.Id, user.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUnsettledAbsenceRegistrations_UserWithUnsettledAbsenceRegistrations_ReturnsList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);

        var registration = TestEntityFactory.CreateLogpunchRegistration(Guid.NewGuid(), user.Id, RegistrationType.Leave, null, DateTimeOffset.UtcNow, null, user.Id, null, DateTimeOffset.UtcNow, RegistrationStatus.Awaiting, null, null, null);
        context.Registrations.Add(registration);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUnsettledAbsenceRegistrations(user.Id, user.Id);

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal(registration.Id, result.First().Id);
    }

    [Fact]
    public async Task GetUnsettledAbsenceRegistrations_ThrowsInvalidOperationExceptionForNonexistentEmployee()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var nonExistentEmployeeId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUnsettledAbsenceRegistrations(user.Id, nonExistentEmployeeId));
    }

    [Fact]
    public async Task AbsenceOverviewQuery_ValidParameters_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var response = await service.AbsenceOverviewQuery(user.Id, user.Id, true, false, null, null, "week", "current", "day", "none", "Leave");

        // Assert
        Assert.NotNull(response);
        Assert.Contains("absenceType=Leave", response.QueryString);
    }

    [Fact]
    public async Task AbsenceOverviewQuery_CustomDates_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow.AddDays(-7);
        var endDate = DateTimeOffset.UtcNow;

        // Act
        var response = await service.AbsenceOverviewQuery(user.Id, user.Id, true, false, startDate, endDate, "custom", "custom", "day", "none", "Sickness");

        // Assert
        Assert.NotNull(response);
        Assert.Contains($"startDate={startDate.DateTime.ToShortDateString()}", response.QueryString);
    }

    [Fact]
    public async Task AbsenceOverviewQuery_InvalidAbsenceType_ThrowsArgumentException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var user = TestEntityFactory.CreateLogpunchUser(Guid.NewGuid(), "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow;
        var endDate = DateTimeOffset.UtcNow.AddDays(-7);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.AbsenceOverviewQuery(user.Id, user.Id, true, false, startDate, endDate, "custom", "custom", "day", "none", "InvalidType"));
    }

}
