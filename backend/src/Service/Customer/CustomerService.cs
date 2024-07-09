using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Shared;

namespace Infrastructure.Customer;

public class CustomerService(LogpunchDbContext dbContext) : ICustomerService
{

    /// <summary>
    /// Gets a list of most recent time registrations for a consultant
    /// </summary>
    /// <param name="dbEntry">All Consultant Customers in the database</param>
    /// <param name="consultantId">The consultant id</param>
    /// <returns>Amount of time registrations</returns>
    public async Task<List<int>> GetMostRecent(IQueryable<EmployeeClientRelation> dbEntry)
    {
        var recentActivities = await dbContext.Registrations
            .Where(tr => dbEntry.Any(cc => cc.Id == tr.LogpunchTask))
            .GroupBy(tr => tr.LogpunchTask)
            .Select(g => new
            {
                ConsultantCustomerId = g.Key,
                MostRecentDate = g.Max(tr => tr.RegistrationDate)
            })
            .OrderBy(lt => lt.MostRecentDate)
            .Select(tr => tr.ConsultantCustomerId).Distinct().ToListAsync();

        return recentActivities;
    }


    /// <summary>
    /// Returns a list of customers for a consultant based on either;
    /// 1. Amount of favorites
    /// 2. Most recent time registrations
    /// 3. Alphabetical order
    /// </summary>
    /// <param name="consultantId">The consultant id</param>
    /// <returns>A list of customers</returns>
    public async Task<List<LogpunchClientDto>> GetCustomers(int consultantId)
    {
        var consultantCustomerIdsByRecentActivity = await GetMostRecent(dbContext.EmployeeClientRelations);

        var consultantCustomers = await dbContext.EmployeeClientRelations
            .Where(cc => cc.ConsultantId == consultantId)
            .Include(cc => cc.Customer)
            .ToListAsync();

        var sortedCustomers = consultantCustomers
            .OrderByDescending(cc => cc.Favorite)
            .ThenByDescending(cc => cc.ConsultantId == consultantCustomerIdsByRecentActivity.IndexOf(cc.Id))
            .ThenBy(cc => cc.Customer!.Name)
            .Select(cc => new LogpunchClientDto
            {
                Id = cc.Customer!.Id,
                Name = cc.Customer.Name
            })
            .ToList();

        var allCustomersExcludingConsultant = await dbContext.Clients
            .Where(c => c.EmployeeClientRelations != null && c.EmployeeClientRelations.All(cc => cc.ConsultantId != consultantId))
            .Select(c => new LogpunchClientDto
            {
                Id = c.Id,
                Name = c.Name
            }).Distinct()
            .ToListAsync();

        var allCustomers = sortedCustomers.Concat(allCustomersExcludingConsultant).ToList();

        return allCustomers;
    }
}
