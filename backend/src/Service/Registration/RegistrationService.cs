using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Shared;

namespace Infrastructure;

public class RegistrationService : IRegistrationService
{
    private readonly LogpunchDbContext _dbContext;

    public RegistrationService(LogpunchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // User functions

    // Work

    public async Task<LogpunchRegistrationDto> CreateWorkRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset start, DateTimeOffset end, string? firstComment, string? secondComment)
    {
        if (start > end)
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

        TimeSpan timeSpan = end - start;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        RegistrationType registrationType = RegistrationType.Work;
        RegistrationStatus registrationStatus = RegistrationStatus.Open;

        var creationTime = DateTimeOffset.UtcNow;
        var registration = new LogpunchRegistration(employee.Id, registrationType, totalMinutes, start, end, creator.Id, client.Id, creationTime, registrationStatus, firstComment, secondComment, null);
        await _dbContext.Registrations.AddAsync(registration);
        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> StartWorkRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment)
    {
        LogpunchUser employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        if (OngoingRegistrationExists(employeeId))
        {
            throw new InvalidOperationException("You cannot start a new registration if an ongoing registration already exists");
        }

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

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> EndWorkRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment)
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
        registration.Status = RegistrationStatus.Open;
        registration.SecondComment = secondComment;

        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    // Transport

    public async Task<LogpunchRegistrationDto> CreateTransportationRegistration(Guid userId, Guid employeeId, Guid? clientId, DateTimeOffset start, DateTimeOffset end, string? firstComment, string? secondComment)
    {
        if (start > end)
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

        TimeSpan timeSpan = end - start;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        RegistrationType registrationType = RegistrationType.Transportation;
        RegistrationStatus registrationStatus = RegistrationStatus.Open;

        var creationTime = DateTimeOffset.UtcNow;
        var registration = new LogpunchRegistration(employee.Id, registrationType, totalMinutes, start, end, creator.Id, client.Id, creationTime, registrationStatus, firstComment, secondComment, null);
        await _dbContext.Registrations.AddAsync(registration);
        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> StartTransportationRegistration(Guid userId, Guid employeeId, Guid? clientId, string? firstComment)
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

        RegistrationType registrationType = RegistrationType.Transportation;
        RegistrationStatus registrationStatus = RegistrationStatus.Ongoing;
        var creationTime = DateTimeOffset.UtcNow;
        var startTime = creationTime;

        var registration = new LogpunchRegistration(employee.Id, registrationType, null, startTime, null, creator.Id, client?.Id, creationTime, registrationStatus, firstComment, null, null);
        await _dbContext.Registrations.AddAsync(registration);
        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> EndTransportationRegistration(Guid userId, Guid employeeId, Guid registrationId, string? secondComment)
    {
        LogpunchUser employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        if (OngoingRegistrationExists(employeeId))
        {
            throw new InvalidOperationException("You cannot start a new registration if an ongoing registration already exists");
        }

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
        registration.Status = RegistrationStatus.Open;
        registration.SecondComment = secondComment;

        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    // Confirm & correct

    public async Task<LogpunchRegistrationDto> EmployeeConfirmationRegistration(Guid userId, Guid registrationId)
    {
        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        LogpunchRegistration registration = _dbContext.Registrations.FirstOrDefault(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found");

        if (user.Id != registration.EmployeeId)
        {
            throw new InvalidOperationException("You can only confirm your own registrations");
        }

        if (registration.Status != RegistrationStatus.Open)
        {
            throw new InvalidOperationException("You can only confirm an open registration");
        }

        registration.Status = RegistrationStatus.Awaiting;

        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> EmployeeCorrectionRegistration(Guid userId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId)
    {
        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        LogpunchRegistration existingRegistration = _dbContext.Registrations.FirstOrDefault(r => r.Id == correctionOfId)
            ?? throw new InvalidOperationException("Registration not found");

        LogpunchClient? client = default;

        if (userId != existingRegistration.EmployeeId)
        {
            throw new InvalidOperationException("You can only make corrections of your own registrations");
        }

        if (existingRegistration.Status != RegistrationStatus.Open)
        {
            throw new InvalidOperationException($"You can only make corrections of an open registration. This registrations status is {existingRegistration.Status}");
        }

        if (existingRegistration.CorrectionOf is not null)
        {
            throw new InvalidOperationException($"You cannot create a correction of a correction. It must point back at the original registration: {existingRegistration.CorrectionOfId}");
        }

        if (start > end)
        {
            throw new InvalidOperationException("Start cannot be later than end");
        }

        if (clientId is not null)
        {
            client = _dbContext.Clients.FirstOrDefault(c => c.Id == clientId)
            ?? throw new InvalidOperationException("Client not found");

            EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
            c.EmployeeId == userId
            && c.ClientId == clientId) ?? throw new InvalidOperationException("No relation exists between employee and client");
        }

        TimeSpan timeSpan = end - start;
        int totalMinutes = (int)timeSpan.TotalMinutes;

        RegistrationType registrationType = RegistrationType.Work;
        RegistrationStatus registrationStatus = RegistrationStatus.Open;

        var creationTime = DateTimeOffset.UtcNow;

        var correctionRegistration = new LogpunchRegistration(user.Id, registrationType, totalMinutes, start, end, user.Id, client?.Id, creationTime, registrationStatus, firstComment, secondComment, existingRegistration.Id);
        await _dbContext.Registrations.AddAsync(correctionRegistration);
        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(correctionRegistration.Id, correctionRegistration.EmployeeId, correctionRegistration.Type.ToString(), correctionRegistration.Amount, correctionRegistration.Start, correctionRegistration.End, correctionRegistration.CreatorId, correctionRegistration.ClientId, correctionRegistration.CreationTime, correctionRegistration.Status.ToString(), correctionRegistration.FirstComment, correctionRegistration.SecondComment, correctionRegistration.CorrectionOfId);

        return registrationDto;
    }

    // Admin functions

    public async Task<LogpunchRegistrationDto> CreateNonWorkRegistration(Guid userId, Guid employeeId, DateTimeOffset start, DateTimeOffset end, string type, string? firstComment, string? secondComment)
    {
        RegistrationType registrationType = RegistrationTypeConverter.ConvertStringToEnum(type);

        if (registrationType != RegistrationType.Leave || registrationType != RegistrationType.Sickness || registrationType != RegistrationType.Vacation)
        {
            throw new InvalidOperationException($"Wrong registration type chosen: {type}. It has to be one of the non-work registrations: ({RegistrationType.Leave}, {RegistrationType.Sickness} or {RegistrationType.Vacation})");
        }

        if (start > end)
        {
            throw new InvalidOperationException("Start cannot be later than end");
        }

        LogpunchUser employee = _dbContext.Users.FirstOrDefault(u => u.Id == employeeId)
            ?? throw new InvalidOperationException("Employee not found");

        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        if (user.Role != UserRole.Admin)
        {
            throw new InvalidOperationException($"Only admins can create non-work registrations ({RegistrationType.Leave}, {RegistrationType.Sickness} or {RegistrationType.Vacation})");
        }

        TimeSpan timeSpan = end - start;
        int totalDays = (int)timeSpan.TotalDays;

        RegistrationStatus registrationStatus = RegistrationStatus.Open;

        var creationTime = DateTimeOffset.UtcNow;

        var registration = new LogpunchRegistration(employee.Id, registrationType, totalDays, start, end, user.Id, null, creationTime, registrationStatus, firstComment, secondComment, null);
        await _dbContext.Registrations.AddAsync(registration);
        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> UpdateRegistrationStatus(Guid userId, Guid registrationId, string newStatus)
    {
        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");


        if (user.Role != UserRole.Admin)
        {
            throw new InvalidOperationException("Permission denied. Only an admin can update the status.");
        }

        LogpunchRegistration registration = _dbContext.Registrations.FirstOrDefault(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found");

        if (registration.CorrectionOf is not null)
        {
            throw new InvalidOperationException("It is not permitted to change the type of a correction. Only the original registrations type can be changed");
        }

        List<LogpunchRegistration> corrections = _dbContext.Registrations.Where(r => r.CorrectionOfId == registrationId).ToList();
        List<LogpunchRegistration> registrationAndCorrections = [registration];
        registrationAndCorrections.AddRange(corrections);

        if (registration.Status == RegistrationStatus.Settled)
        {
            throw new InvalidOperationException("Once a registration is settled its status cannot change");
        }

        if (!Enum.IsDefined(typeof(RegistrationStatus), newStatus))
        {
            throw new InvalidOperationException("Invalid status value");
        }

        RegistrationStatus registrationStatus = RegistrationStatusConverter.ConvertStringToEnum(newStatus);

        foreach (var reg in registrationAndCorrections)
        {
            reg.Status = registrationStatus;
        }

        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> ChangeRegistrationType(Guid userId, Guid registrationId, string newType)
    {
        LogpunchUser user = _dbContext.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        if (user.Role != UserRole.Admin)
        {
            throw new InvalidOperationException("Permission denied. Only an admin can update the type.");
        }

        LogpunchRegistration registration = _dbContext.Registrations.FirstOrDefault(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found");

        if (registration.CorrectionOf is not null)
        {
            throw new InvalidOperationException("It is not permitted to change the type of a correction. Only the original registrations type can be changed");
        }

        List<LogpunchRegistration> corrections = _dbContext.Registrations.Where(r => r.CorrectionOfId == registrationId).ToList();
        List<LogpunchRegistration> registrationAndCorrections = [registration];
        registrationAndCorrections.AddRange(corrections);

        if (registration.Type == RegistrationType.Work || RegistrationTypeConverter.ConvertStringToEnum(newType) == RegistrationType.Work)
        {
            throw new("Registrations can't be changed either into or from work registrations");
        }

        if (registration.Status == RegistrationStatus.Settled)
        {
            throw new InvalidOperationException("Once a registration is settled its type cannot change");
        }

        if (!Enum.IsDefined(typeof(RegistrationType), newType))
        {
            throw new InvalidOperationException("Invalid type value");
        }

        RegistrationType registrationType = RegistrationTypeConverter.ConvertStringToEnum(newType);

        foreach (var reg in registrationAndCorrections)
        {
            reg.Type = registrationType;
        }

        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(registration.Id, registration.EmployeeId, registration.Type.ToString(), registration.Amount, registration.Start, registration.End, registration.CreatorId, registration.ClientId, registration.CreationTime, registration.Status.ToString(), registration.FirstComment, registration.SecondComment, registration.CorrectionOfId);

        return registrationDto;
    }

    public async Task<LogpunchRegistrationDto> AdminCorrectionRegistration(Guid userId, Guid employeeId, DateTimeOffset start, DateTimeOffset end, Guid? clientId, string? firstComment, string? secondComment, Guid correctionOfId)
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

        if (existingRegistration.CorrectionOf is not null)
        {
            throw new InvalidOperationException($"You cannot create a correction of a correction. It must point back at the original registration: {existingRegistration.CorrectionOfId}");
        }

        EmployeeClientRelation? employeeClientRelation = _dbContext.EmployeeClientRelations.FirstOrDefault(c =>
            c.EmployeeId == employeeId
            && c.ClientId == clientId) ?? throw new InvalidOperationException("No relation exists between employee and client");

        TimeSpan timeSpan = end - start;
        int time;

        switch (existingRegistration.Type)
        {
            case RegistrationType.Work:
                time = (int)timeSpan.TotalMinutes;
                break;

            case RegistrationType.Transportation:
                time = (int)timeSpan.TotalMinutes;
                break;

            default:
                time = (int)timeSpan.TotalDays;
                break;
        }

        RegistrationType registrationType = existingRegistration.Type;
        RegistrationStatus registrationStatus = existingRegistration.Status;

        var creationTime = DateTimeOffset.UtcNow;

        var correctionRegistration = new LogpunchRegistration(employee.Id, registrationType, time, start, end, user.Id, client.Id, creationTime, registrationStatus, firstComment, secondComment, existingRegistration.Id);
        await _dbContext.Registrations.AddAsync(correctionRegistration);
        await _dbContext.SaveChangesAsync();

        LogpunchRegistrationDto registrationDto = new(correctionRegistration.Id, correctionRegistration.EmployeeId, correctionRegistration.Type.ToString(), correctionRegistration.Amount, correctionRegistration.Start, correctionRegistration.End, correctionRegistration.CreatorId, correctionRegistration.ClientId, correctionRegistration.CreationTime, correctionRegistration.Status.ToString(), correctionRegistration.FirstComment, correctionRegistration.SecondComment, correctionRegistration.CorrectionOfId);

        return registrationDto;
    }

    private bool OngoingRegistrationExists(Guid employeeId)
    {
        bool result = _dbContext.Registrations.Where(r => r.EmployeeId == employeeId && r.Status == RegistrationStatus.Ongoing).Any();
        return result;
    }
}
