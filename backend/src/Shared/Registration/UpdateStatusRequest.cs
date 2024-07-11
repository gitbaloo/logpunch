namespace Shared;

public class UpdateStatusRequest
{
    public Guid RegistrationId { get; set; }
    public int Status { get; set; }
}
