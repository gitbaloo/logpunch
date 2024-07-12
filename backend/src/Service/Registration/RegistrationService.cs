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

    public async Task<LogpunchRegistration> CreateRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset startTime, DateTimeOffset endTime, string? firstComment, string? secondComment)
    {
        if (startTime > endTime)
        {
            throw new InvalidOperationException("Start cannot be later than end");
        }

        LogpunchUser employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchUser creator = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        LogpunchClient client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId)
            ?? throw new InvalidOperationException("Client not found");

        EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
            c.EmployeeId == employeeId
            && c.ClientId == clientId) ?? throw new InvalidOperationException("No relation exists between employee and client");

        TimeSpan timeSpan = endTime - startTime;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        RegistrationType registrationType = RegistrationType.Work;
        RegistrationStatus registrationStatus = RegistrationStatus.Awaiting;

        var creationTime = DateTimeOffset.UtcNow;
        var startTimeUtc = startTime.ToUniversalTime();
        var endTimeUtc = endTime.ToUniversalTime();
        var registration = new LogpunchRegistration(employee.Id, registrationType, totalMinutes, startTimeUtc, endTimeUtc, creator.Id, client.Id, creationTime, registrationStatus, firstComment, secondComment, null);
        await _dbContext.Registrations.AddAsync(registration);
        await _dbContext.SaveChangesAsync();

        return registration;
    }

    public async Task<LogpunchRegistration> StartShiftRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment)
    {
        LogpunchUser employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchUser creator = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        LogpunchClient? client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId);

        if (client is not null)
        {
            EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
                c.EmployeeId == employeeId
                && c.ClientId == clientId) ?? throw new InvalidOperationException("No relation exists between employee and client");
        }

        RegistrationType registrationType = RegistrationType.Work;
        RegistrationStatus registrationStatus = RegistrationStatus.Ongoing;
        var creationTime = DateTimeOffset.UtcNow;
        var startTime = creationTime;

        var registration = new LogpunchRegistration(employee.Id, registrationType, null, startTime, null, creator.Id, client?.Id, creationTime, registrationStatus, firstComment, null, null);
        await _dbContext.Registrations.AddAsync(registration);
        await _dbContext.SaveChangesAsync();

        return registration;
    }

    public async Task<LogpunchRegistration> EndShiftRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment)
    {
        LogpunchUser employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        LogpunchRegistration registration = _dbContext.Registrations.FirstOrDefault(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found");

        if (user != employee && user.Role != UserRole.Admin)
        {
            throw new InvalidOperationException("User and employee is not the same. Only an admin can end another employees shift");
        }

        if (registration.Status != RegistrationStatus.Ongoing)
        {
            throw new InvalidOperationException("Registration status is not 'Ongoing'");
        }

        var endTime = DateTimeOffset.UtcNow;

        TimeSpan timeSpan = endTime - registration.Start;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        registration.Amount = totalMinutes;
        registration.End = endTime;
        registration.Status = RegistrationStatus.Awaiting;
        registration.SecondComment = secondComment;

        await _dbContext.SaveChangesAsync();

        return registration;
    }

    public async Task<LogpunchRegistration> UpdateRegistrationStatus(Guid userId, Guid registrationId, int newStatus)
    {
        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        if (user.Role != UserRole.Admin)
        {
            throw new InvalidOperationException("Permission denied. Only an admin can update the status.");
        }

        LogpunchRegistration registration = _dbContext.Registrations.FirstOrDefault(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found");

        if (registration.Status == RegistrationStatus.Settled)
        {
            throw new InvalidOperationException("Once a registration is settled its status cannot change");
        }

        if (!Enum.IsDefined(typeof(RegistrationStatus), newStatus))
        {
            throw new InvalidOperationException("Invalid status value");
        }

        registration.Status = (RegistrationStatus)newStatus;

        await _dbContext.SaveChangesAsync();

        return registration;
    }
    public async Task<LogpunchRegistration> ChangeRegistrationType(Guid userId, Guid registrationId, int newType)
    {
        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        if (user.Role != UserRole.Admin)
        {
            throw new InvalidOperationException("Permission denied. Only an admin can update the type.");
        }

        LogpunchRegistration registration = _dbContext.Registrations.FirstOrDefault(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found");

        if (registration.Status == RegistrationStatus.Settled)
        {
            throw new InvalidOperationException("Once a registration is settled its type cannot change");
        }

        if (!Enum.IsDefined(typeof(RegistrationType), newType))
        {
            throw new InvalidOperationException("Invalid type value");
        }

        registration.Type = (RegistrationType)newType;

        await _dbContext.SaveChangesAsync();

        return registration;
    }

    public async Task<LogpunchRegistration> CreateCorrectionRegistration(Guid userId, Guid employeeId, int type, DateTimeOffset start, DateTimeOffset end, Guid? clientId, int status, string? firstComment, string? secondComment, Guid correctionOfId)
    {
        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        if (user.Role != UserRole.Admin)
        {
            throw new InvalidOperationException("Permission denied. Only an admin can update the type.");
        }

        if (start > end)
        {
            throw new InvalidOperationException("Start cannot be later than end");
        }

        LogpunchUser employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchClient client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId)
            ?? throw new InvalidOperationException("Client not found");

        LogpunchRegistration existingRegistration = _dbContext.Registrations.FirstOrDefault(r => r.Id == correctionOfId)
            ?? throw new InvalidOperationException("Registration not found");

        EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
            c.EmployeeId == employeeId
            && c.ClientId == clientId) ?? throw new InvalidOperationException("No relation exists between employee and client");

        TimeSpan timeSpan = end - start;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        RegistrationType registrationType = RegistrationType.Work;
        RegistrationStatus registrationStatus = RegistrationStatus.Awaiting;

        var creationTime = DateTimeOffset.UtcNow;

        var correctionRegistration = new LogpunchRegistration(employee.Id, registrationType, totalMinutes, start, end, user.Id, client.Id, creationTime, registrationStatus, firstComment, secondComment, existingRegistration.Id);
        await _dbContext.Registrations.AddAsync(correctionRegistration);
        await _dbContext.SaveChangesAsync();

        return correctionRegistration;

    }
}
