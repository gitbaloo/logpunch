namespace Shared;

public class GroupByObject
{
    public string Name { get; set; }
    public double Total { get; set; }
    public List<ThenByObject> ThenByObjects { get; set; }

    // Constructor without ThenByObjects
    public GroupByObject(string name, double total)
    {
        Name = name;
        Total = total;
        ThenByObjects = new List<ThenByObject>(); // Initialize with empty list or null based on your requirement
    }

    // Constructor with ThenByObjects
    public GroupByObject(string name, double total, List<ThenByObject> thenByObjects)
    {
        Name = name;
        Total = total;
        ThenByObjects = thenByObjects;
    }
}
