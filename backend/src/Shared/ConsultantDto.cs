namespace Shared;

public class ConsultantDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? DefaultQuery { get; set; }


    public ConsultantDto(int id, string firstName, string lastName, string email, string? defaultQuery)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DefaultQuery = defaultQuery;
    }

    public ConsultantDto()
    {
        FirstName = "";
        LastName = "";
        Email = "";
        DefaultQuery = null;
    }
}
