namespace Domain
{
    public enum UserRole
    {
        Employee,
        Admin
    }

    public class UserRoleConverter
    {
        public static UserRole ConvertStringToEnum(string userRoleString)
        {
            if (Enum.TryParse(userRoleString, out UserRole userRole))
            {
                return userRole;
            }
            else
            {
                throw new ArgumentException($"Invalid registration type: {userRoleString}");
            }
        }
    }
}
