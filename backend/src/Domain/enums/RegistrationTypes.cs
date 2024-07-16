namespace Domain
{
    public enum RegistrationType
    {
        Work,
        Vacation,
        Sickness,
        Leave,
        Transportation
    }

    public class RegistrationTypeConverter
    {
        public static RegistrationType ConvertStringToEnum(string registrationTypeString)
        {
            if (Enum.TryParse(registrationTypeString, out RegistrationType registrationType))
            {
                return registrationType;
            }
            else
            {
                throw new ArgumentException($"Invalid registration type: {registrationTypeString}");
            }
        }
    }
}
