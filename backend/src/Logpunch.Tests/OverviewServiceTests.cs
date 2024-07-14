using System;
using System.Threading.Tasks;
using Domain;
using Infrastructure.Overview;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Shared;
using Xunit;

public class OverviewServiceTests
{
    [Fact]
    public async Task OverviewQuery_ValidData_ReturnsOverviewResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: "LogpunchDb")
            .Options;

        using var context = new LogpunchDbContext(options);
        var service = new OverviewService(context);

        var userId = Guid.NewGuid();
        context.Users.Add(TestEntityFactory.CreateLogpunchUser(userId, "user@example.com", "password", "FirstName", "LastName", null, UserRole.Employee));
        await context.SaveChangesAsync();

        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        // Act
        var response = await service.WorkOverviewQuery(userId, true, false, false, startDate, endDate, "custom", "custom", "day", "none", "Work");

        // Assert
        Assert.NotNull(response);
        Assert.Contains("sort_asc=true", response.QueryString);
    }

    // Add more tests as necessary...
}
