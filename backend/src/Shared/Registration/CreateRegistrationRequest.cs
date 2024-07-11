namespace Shared;

public class CreateRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public Guid? ClientId { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string? FirstComment { get; set; }
    public string? SecondComment { get; set; }
}
