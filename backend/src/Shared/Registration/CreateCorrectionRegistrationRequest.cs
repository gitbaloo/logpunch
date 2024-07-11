namespace Shared;

public class CreateCorrectionRegistrationRequest
{
    public Guid EmployeeId { get; set; }
    public int Type { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public Guid? ClientId { get; set; }
    public int Status { get; set; }
    public string? FirstComment { get; set; }
    public string? SecondComment { get; set; }
    public Guid CorrectionOfId { get; set; }
}
