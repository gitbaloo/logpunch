using System;
using System.Threading.Tasks;
using Domain;
using Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;
using static Service.Tests.TestUtilities;

namespace Service.Tests.TimeRegistration;

[TestSubject(typeof(RegistrationService))]
public class TimeRegistrationServiceTest
{
    private readonly RegistrationService _service;
    private readonly LogpunchDbContext _dbContext;

    /**
     * Constructor for the test class that creates a new in-memory database for each test case
     * and a mock configuration and transport service.
     * Creates 20 test consultants and 20 test customers.
     */
    public TimeRegistrationServiceTest()
    {
        var options = new DbContextOptionsBuilder<LogpunchDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new LogpunchDbContext(options);

        _service = new RegistrationService(_dbContext);

        var consultants = AddTestConsultants();
        var customers = AddTestCustomers();
        _dbContext.AddRange(consultants);
        _dbContext.AddRange(customers);
        _dbContext.SaveChanges();
    }

    /**
     * Tests that round up is working as expected.
     */
    [Fact]
    public async Task ROUND_UP()
    {
        // Arrange
        const double hours = 1.51;
        const double expected = 2.0;
        var expectedCustomer = "TestCompany1";

        var customer = await _dbContext.Clients.FirstOrDefaultAsync(customer => customer.Id == 1);
        if (customer is null)
        {
            Assert.Fail("Customer was null");
            return;
        }

        var consultant = await _dbContext.Users.FirstOrDefaultAsync(consultant => consultant.Id == 1);
        if (consultant is null)
        {
            Assert.Fail("Consultant was null");
            return;
        }


        var consultantCustomer1 = new ConsultantCustomer(consultant, customer);
        _dbContext.EmployeeClientRelations.Add(consultantCustomer1);
        await _dbContext.SaveChangesAsync();

        var consultantCustomer = await _dbContext.EmployeeClientRelations
            .Include(cc => cc.Customer)
            .FirstAsync();

        // Act
        var registeredTime = await _service.RegisterTime(consultantCustomer.ConsultantId, consultantCustomer.CustomerId, hours);

        // Assert
        Assert.Equal(expected, registeredTime.Hours);
        Assert.Equal(expectedCustomer, consultantCustomer.Customer.Name);
    }

    /**
     * Tests if registered time is automatically set to today.
     */
    [Fact]
    public async Task DATE_AUTOFILLED_FOR_TODAY()
    {
        // Arrange
        var expectedDate = DateTime.Today.Date;

        // Act
        var timeRegistration = await _service.CreateRegistration(1, 2, 3);
        var actualDate = timeRegistration.RegistrationDate.Date;

        // Assert
        Assert.Equal(expectedDate, actualDate);
    }
}
