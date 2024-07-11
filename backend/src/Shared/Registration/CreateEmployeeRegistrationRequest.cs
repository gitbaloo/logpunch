namespace Shared;

public class CreateEmployeeRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public Guid? ClientId { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string? InternalComment { get; set; }
}
