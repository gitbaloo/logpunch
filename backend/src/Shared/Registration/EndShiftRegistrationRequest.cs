namespace Shared;

public class EndShiftRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public Guid RegistrationId { get; set; }
    public string? SecondComment { get; set; }
}
