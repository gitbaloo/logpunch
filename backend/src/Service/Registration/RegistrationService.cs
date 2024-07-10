using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure;

public class RegistrationService : IRegistrationService
{
    private readonly LogpunchDbContext _dbContext;

    public RegistrationService(LogpunchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LogpunchRegistration> EmployeeCreateRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? internalComment)
    {
        if (startTime > endTime)
        {
            throw new InvalidOperationException("Start cannot be later than end");
        }

        LogpunchUser employee = _dbContext.Users.FirstOrDefault(c => c.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchUser creator = _dbContext.Users.FirstOrDefault(c => c.Id == userId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchClient client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId)
            ?? throw new InvalidOperationException("Client not found");

        EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
            c.EmployeeId == employeeId
            && c.ClientId == clientId) ?? throw new InvalidOperationException("No relation exists between employee and client");

        RegistrationType registrationType = RegistrationType.Work;

        TimeSpan timeSpan = endTime - startTime;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        RegistrationStatus registrationStatus = RegistrationStatus.Awaiting;

        var creationTime = DateTimeOffset.Now;

        var registeredTime = new LogpunchRegistration(employee, creator, client, null, registrationType, totalMinutes, startTime, endTime, creationTime, registrationStatus, internalComment, null);
        await _dbContext.Registrations.AddAsync(registeredTime);
        await _dbContext.SaveChangesAsync();

        return registeredTime;
    }

    public async Task<LogpunchRegistration> StartShiftRegistration(Guid userId, Guid employeeId, Guid? clientId, string? internalComment)
    {
        LogpunchUser employee = _dbContext.Users.FirstOrDefault(c => c.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        var creator = employee; // change to check who is logged in

        LogpunchClient? client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId);

        if (client is not null)
        {
            EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
                c.EmployeeId == employeeId
                && c.ClientId == clientId) ?? throw new InvalidOperationException("No relation exists between employee and client");
        }

        RegistrationType registrationType = RegistrationType.Work;
        RegistrationStatus registrationStatus = RegistrationStatus.Ongoing;
        var startShiftComment = "Start of shift comment: " + Environment.NewLine + internalComment;
        var creationTime = DateTimeOffset.Now;
        var startTime = creationTime;

        var registeredTime = new LogpunchRegistration(employee, creator, client, null, registrationType, null, startTime, null, creationTime, registrationStatus, startShiftComment, null);
        await _dbContext.Registrations.AddAsync(registeredTime);
        await _dbContext.SaveChangesAsync();

        return registeredTime;
    }

    public async Task<LogpunchRegistration> EndShiftRegistration(Guid userId, Guid employeeId, Guid registrationId, string? internalComment)
    {
        LogpunchUser employee = _dbContext.Users.FirstOrDefault(c => c.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchRegistration registration = _dbContext.Registrations.FirstOrDefault(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found");

        if (registration.Status != RegistrationStatus.Ongoing)
        {
            throw new InvalidOperationException("Registration status is not 'Ongoing'");
        }

        var endTime = DateTimeOffset.Now;

        TimeSpan timeSpan = endTime - registration.Start;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        registration.Amount = totalMinutes;
        registration.End = endTime;
        registration.Status = RegistrationStatus.Awaiting;

        var startShiftComment = registration.InternalComment;

        if (registration.InternalComment is not null)
        {
            registration.InternalComment = startShiftComment + Environment.NewLine + Environment.NewLine + "End of shift comment:" + Environment.NewLine + internalComment;
        }
        else
        {
            registration.InternalComment = "End of shift comment:" + Environment.NewLine + internalComment;
        }



        await _dbContext.SaveChangesAsync();

        return registration;
    }


}
