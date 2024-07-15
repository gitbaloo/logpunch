namespace Shared
{
    public class CreateAbsenceRegistrationRequest
    {
        public Guid EmployeeId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string Type { get; set; }
        public string? FirstComment { get; set; }
        public string? SecondComment { get; set; }

        public CreateAbsenceRegistrationRequest()
        {
            Type = "";
        }
    }
}
