namespace Shared
{
    public class LogpunchRegistrationDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string Type { get; set; }
        public int? Amount { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset? End { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? ClientId { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public string Status { get; set; }
        public string? FirstComment { get; set; }
        public string? SecondComment { get; set; }
        public Guid? CorrectionOfId { get; set; }

        public LogpunchRegistrationDto(Guid id, Guid employeeId, string type, int? amount, DateTimeOffset start, DateTimeOffset? end, Guid creatorId, Guid? clientId, DateTimeOffset creationTime, string status, string? firstComment, string? secondComment, Guid? correctionOfId)
        {
            Id = id;
            EmployeeId = employeeId;
            Type = type;
            Amount = amount;
            Start = start;
            End = end;
            CreatorId = creatorId;
            ClientId = clientId;
            CreationTime = creationTime;
            Status = status;
            FirstComment = firstComment;
            SecondComment = secondComment;
            CorrectionOfId = correctionOfId;
        }
    }
}
