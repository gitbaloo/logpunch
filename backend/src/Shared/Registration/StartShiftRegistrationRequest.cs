namespace Shared;

public class StartShiftRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public Guid? ClientId { get; set; }
    public string? InternalComment { get; set; }
}
