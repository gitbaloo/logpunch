namespace Domain;

public class Customer : Entity
{
    public string Name { get; set; }

    public ICollection<ConsultantCustomer>? ConsultantCustomers { get; set; }

    public Customer(string name)
    {
        Name = name;
    }


}
