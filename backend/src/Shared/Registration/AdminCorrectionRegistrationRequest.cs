namespace Shared
{
    public class AdminCorrectionRegistrationRequest
    {
        public Guid EmployeeId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public Guid? ClientId { get; set; }
        public string? FirstComment { get; set; }
        public string? SecondComment { get; set; }
        public Guid CorrectionOfId { get; set; }
    }
}
