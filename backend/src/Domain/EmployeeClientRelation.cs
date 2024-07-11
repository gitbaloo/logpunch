using Newtonsoft.Json;

namespace Domain;

public class EmployeeClientRelation : Entity
{
    public Guid EmployeeId { get; set; }
    public Guid ClientId { get; set; }
    [JsonIgnore]
    public LogpunchUser? Employee { get; set; }
    [JsonIgnore]
    public LogpunchClient? Client { get; set; }

    public EmployeeClientRelation(LogpunchUser employee, LogpunchClient client)
    {
        Employee = employee;
        EmployeeId = employee.Id;
        Client = client;
        ClientId = client.Id;
    }
    public EmployeeClientRelation()
    {

    }
}
