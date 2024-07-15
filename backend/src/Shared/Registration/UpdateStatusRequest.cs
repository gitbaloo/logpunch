namespace Shared
{
    public class UpdateStatusRequest
    {
        public Guid RegistrationId { get; set; }
        public string Status { get; set; }

        public UpdateStatusRequest()
        {
            Status = string.Empty;
        }

        public UpdateStatusRequest(Guid registrationId, string status)
        {
            RegistrationId = registrationId;
            Status = status;
        }
    }
}
