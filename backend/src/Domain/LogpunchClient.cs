using Newtonsoft.Json;
namespace Domain;

public class LogpunchClient : Entity
{
    public string Name { get; set; }

    [JsonIgnore]
    public ICollection<EmployeeClientRelation>? EmployeeClientRelations { get; set; }

    public LogpunchClient(string name)
    {
        Name = name;
    }

    public LogpunchClient()
    {
        Name = string.Empty;
    }
}
