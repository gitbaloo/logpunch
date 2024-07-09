namespace Domain;

public class LogpunchClient : Entity
{
    public string Name { get; set; }

    public ICollection<EmployeeClientRelation>? EmployeeClientRelations { get; set; }

    public LogpunchClient(string name)
    {
        Name = name;
    }


}
