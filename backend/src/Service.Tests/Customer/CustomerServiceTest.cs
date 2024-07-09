using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Infrastructure.Customer;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;
using static Service.Tests.TestUtilities;

namespace Service.Tests.Customer;

[TestSubject(typeof(CustomerService))]
public class CustomerServiceTest
{
    private readonly LogpunchDbContext _dbContext;
    private readonly CustomerService _service;

    public CustomerServiceTest()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new LogpunchDbContext(options);

        _service = new CustomerService(_dbContext);

        var consultants = AddTestConsultants();
        _dbContext.AddRange(consultants);

        var customers = AddTestCustomers();
        _dbContext.AddRange(customers);

        _dbContext.AddRange(AddTestConsultantCustomers(customers, consultants));
        _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GET_CUSTOMERS()
    {
        // Arrange
        const int expected = 20;

        // Act
        var customers = await _service.GetCustomers(1);

        // Assert
        Assert.Equal(expected, customers.Count);
    }

    [Fact]
    public async Task GET_MOST_RECENT()
    {
        // Arrange
        var hours = 3;
        var consultantId = 2;
        var consultantCustomerId = 3;
        Domain.LogpunchRegistration time = new Domain.LogpunchRegistration(hours, consultantCustomerId);

        _dbContext.Registrations.Add(time);

        await _dbContext.SaveChangesAsync();
        var expectedCustomer = "TestCompany1";

        // Act
        var customers = await _service.GetCustomers(consultantId);

        // Assert
        Assert.Equal(expectedCustomer, customers.First().Name);
    }

    [Fact]
    public async Task GET_FAVORITE()
    {
        // ARRANGE
        var expected = "TestCompany19";

        // ACT
        var consultantId = 1;
        var result = await _service.GetCustomers(consultantId);

        // ASSERT
        Assert.Equal(expected, result.First().Name);
    }

}
