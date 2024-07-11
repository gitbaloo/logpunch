namespace Shared;

public class CreateCorrectionRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public int Type { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? ClientId { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public int Status { get; set; }
    public string? InternalComment { get; set; }
    public Guid CorrectionOf { get; set; }
}
