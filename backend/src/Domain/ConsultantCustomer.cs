using Newtonsoft.Json;

namespace Domain;

public class ConsultantCustomer : Entity
{
    public int ConsultantId { get; set; }
    public int CustomerId { get; set; }
    public bool Favorite { get; set; }
    [JsonIgnore]
    public Consultant? Consultant { get; set; }
    [JsonIgnore]
    public Customer? Customer { get; set; }

    public ConsultantCustomer(Consultant consultant, Customer customer)
    {
        Consultant = consultant;
        ConsultantId = consultant.Id;
        Customer = customer;
        CustomerId = customer.Id;

    }
    public ConsultantCustomer()
    {

    }
}
