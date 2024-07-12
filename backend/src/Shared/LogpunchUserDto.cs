namespace Shared;

public class LogpunchUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? DefaultQuery { get; set; }
    public string Role { get; set; }

    public LogpunchUserDto(Guid id, string firstName, string lastName, string email, string? defaultQuery, string role)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DefaultQuery = defaultQuery;
        Role = role;
    }

    public LogpunchUserDto()
    {
        FirstName = "";
        LastName = "";
        Email = "";
        DefaultQuery = null;
        Role = "em";
    }
}
