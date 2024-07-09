namespace Shared;

public class CreateEmployeeRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public string Type { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? ClientId { get; set; }
    public DateTimeOffset CreationTime { get; set; }
    public string Status { get; set; }
    public string? InternalComment { get; set; }
}
