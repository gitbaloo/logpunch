using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection.Metadata;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;

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
        [Column("creatorid")]
        public Guid CreatorId { get; set; }
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
        [Column("correctionof_id")]
        public Guid? CorrectionOfId { get; set; }

        [JsonIgnore]
        public LogpunchUser? Employee { get; set; }
        [JsonIgnore]
        public LogpunchUser? Creator { get; set; }
        [JsonIgnore]
        public LogpunchClient? Client { get; set; }
        [JsonIgnore]
        public LogpunchRegistration? CorrectionOf { get; set; }

        public LogpunchRegistration()
        {

        }
        public LogpunchRegistration(Guid employeeId, RegistrationType type, int? amount, DateTimeOffset start, DateTimeOffset? end, Guid creatorId, Guid? clientId, DateTimeOffset creationTime, RegistrationStatus status, string? internalComment, string? externalComment, Guid? correctionOfId)
        {
            EmployeeId = employeeId;
            Type = type;
            Amount = amount;
            Start = start;
            End = end;
            CreatorId = creatorId;
            ClientId = clientId;
            CreationTime = creationTime;
            Status = status;
            InternalComment = internalComment;
            ExternalComment = externalComment;
            CorrectionOfId = correctionOfId;
        }
    }
}
