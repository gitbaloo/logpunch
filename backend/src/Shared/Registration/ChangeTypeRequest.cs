namespace Shared
{
    public class ChangeTypeRequest
    {
        public Guid RegistrationId { get; set; }
        public string Type { get; set; }

        public ChangeTypeRequest()
        {
            Type = string.Empty;
        }

        public ChangeTypeRequest(Guid registrationId, string type)
        {
            RegistrationId = registrationId;
            Type = type;
        }
    }
}
