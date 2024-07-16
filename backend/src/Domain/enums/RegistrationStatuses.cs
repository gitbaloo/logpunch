namespace Domain
{
    public enum RegistrationStatus
    {
        Ongoing,
        Open,
        Awaiting,
        Approved,
        Rejected,
        Settled
    }

    public class RegistrationStatusConverter
    {
        public static RegistrationStatus ConvertStringToEnum(string registrationStatusString)
        {
            if (Enum.TryParse(registrationStatusString, out RegistrationStatus registrationStatus))
            {
                return registrationStatus;
            }
            else
            {
                throw new ArgumentException($"Invalid registration status: {registrationStatusString}");
            }
        }
    }
}
