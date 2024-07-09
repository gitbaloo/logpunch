namespace Shared;

public class UpdateEmployeeRegistrationRequest
{
    public DateTimeOffset End { get; set; }
    public string Status { get; set; }
    public string? InternalComment { get; set; }
}
