using Newtonsoft.Json;

namespace Domain;

public class LogpunchUser : Entity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    [JsonIgnore]
    public string Password { get; set; }
    public string? DefaultQuery { get; set; }
    public UserRole Role { get; set; }

    [JsonIgnore]
    public ICollection<EmployeeClientRelation>? EmployeeClientRelations { get; set; }


    public LogpunchUser(string firstName, string lastName, string email, string password, string? defaultQuery, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        DefaultQuery = defaultQuery;
        Role = role;
    }

    protected LogpunchUser()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
    }
}
