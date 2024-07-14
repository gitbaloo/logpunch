namespace Shared;

public class StartRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public Guid? ClientId { get; set; }
    public string? FirstComment { get; set; }
}
