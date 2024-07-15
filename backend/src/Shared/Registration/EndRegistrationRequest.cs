namespace Shared
{
    public class EndRegistrationRequest
    {
        public Guid? EmployeeId { get; set; }
        public Guid RegistrationId { get; set; }
        public string? SecondComment { get; set; }
    }
}
