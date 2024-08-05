namespace Shared
{
    public class EmployeeCorrectionRegistrationRequest
    {
        public Guid? ClientId { get; set; }
        public string Type { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string? FirstComment { get; set; }
        public string? SecondComment { get; set; }
        public Guid CorrectionOfId { get; set; }

        public EmployeeCorrectionRegistrationRequest()
        {
            Type = string.Empty;
        }
    }
}
