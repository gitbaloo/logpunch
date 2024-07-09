using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection.Metadata;
using Npgsql;
using NpgsqlTypes;

namespace Domain
{
    public class LogpunchRegistration : Entity
    {
        [Column("employeeid")]
        public Guid EmployeeId { get; set; }
        [Column("registration_type")]
        public RegistrationType Type { get; set; }
        [Column("amount")]
        public int? Amount { get; set; }
        [Column("registration_start")]
        public DateTimeOffset Start { get; set; }
        [Column("registration_end")]
        public DateTimeOffset? End { get; set; }
        [Column("created_by_id")]
        public Guid CreatedById { get; set; }
        [Column("clientid")]
        public Guid? ClientId { get; set; }
        [Column("creation_time")]
        public DateTimeOffset CreationTime { get; set; }
        [Column("status_type")]
        public RegistrationStatus Status { get; set; }
        [Column("internal_comment")]
        public string? InternalComment { get; set; }
        [Column("external_comment")]
        public string? ExternalComment { get; set; }
        [Column("correction_of_id")]
        public Guid? CorrectionOfId { get; set; }

        public LogpunchUser? Employee { get; set; }
        public LogpunchUser? Creator { get; set; }
        public LogpunchClient? Client { get; set; }
        public LogpunchRegistration? CorrectionOf { get; set; }

        public LogpunchRegistration(Guid employeeId, RegistrationType type, int? amount, DateTimeOffset start, DateTimeOffset? end, Guid createdById, Guid? clientId, DateTimeOffset creationTime, RegistrationStatus status, string? internalComment, string? externalComment, Guid? correctionOfId)
        {
            EmployeeId = employeeId;
            Type = type;
            Amount = amount;
            Start = start;
            End = end;
            CreatedById = createdById;
            ClientId = clientId;
            CreationTime = creationTime;
            Status = status;
            InternalComment = internalComment;
            ExternalComment = externalComment;
            CorrectionOfId = correctionOfId;
        }

        public LogpunchRegistration(LogpunchUser employee, LogpunchUser creator, LogpunchClient? client, LogpunchRegistration? correctionOf, RegistrationType type, int amount, DateTimeOffset start, DateTimeOffset end, DateTimeOffset creationTime, RegistrationStatus status, string? internalComment, string? externalComment)
        {
            Employee = employee;
            Creator = creator;
            Client = client;
            CorrectionOf = correctionOf;
            EmployeeId = employee.Id;
            Type = type;
            Amount = amount;
            Start = start;
            End = end;
            CreatedById = creator.Id;
            ClientId = client?.Id;
            CreationTime = creationTime;
            Status = status;
            InternalComment = internalComment;
            ExternalComment = externalComment;
            CorrectionOfId = correctionOf?.Id;
        }
    }
}
