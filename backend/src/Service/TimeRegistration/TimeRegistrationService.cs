using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure;

public class TimeRegistrationService : ITimeRegistrationService
{
    private readonly LogpunchDbContext _dbContext;

    public TimeRegistrationService(LogpunchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TimeRegistration> RegisterTime(int consultantId, int customerId, double hours)
    {
        Consultant consultant = _dbContext.Consultants.FirstOrDefault(c => c.Id == consultantId)
            ?? throw new InvalidOperationException("Consultant not found");


        Domain.Customer customer = _dbContext.Customers.FirstOrDefault(c => c.Id == customerId)
            ?? throw new InvalidOperationException("Customer not found");

        ConsultantCustomer? consultantCustomer = _dbContext.ConsultantCustomers.FirstOrDefault(c =>
            c.ConsultantId == consultantId
            && c.CustomerId == customerId);

        if (consultantCustomer is null)
        {
            consultantCustomer = new ConsultantCustomer(consultant, customer);
            _dbContext.ConsultantCustomers.Add(consultantCustomer);

            await _dbContext.SaveChangesAsync(); //TODO: Might be okay to delete
        }

        hours = Math.Ceiling(hours * 2) / 2;
        var registeredTime = new TimeRegistration(hours, consultantCustomer.Id);
        await _dbContext.TimeRegistrations.AddAsync(registeredTime);
        await _dbContext.SaveChangesAsync();

        return registeredTime;
    }
}
