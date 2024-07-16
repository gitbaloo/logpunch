namespace Shared
{
    public class LogpunchUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public LogpunchUserDto(Guid id, string firstName, string lastName, string email, string role)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Role = role;
        }

        public LogpunchUserDto()
        {
            FirstName = "";
            LastName = "";
            Email = "";
            Role = "Employee";
        }
    }
}
