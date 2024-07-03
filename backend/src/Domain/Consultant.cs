using Newtonsoft.Json;
using Service.Login;

namespace Domain;

public class Consultant : Entity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    [JsonIgnore]
    public string Password { get; set; }
    public string? DefaultQuery { get; set; }

    public ICollection<ConsultantCustomer>? ConsultantCustomers { get; set; }


    public Consultant(string firstName, string lastName, string email, string password, string? defaultQuery)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        DefaultQuery = defaultQuery;
    }
}
