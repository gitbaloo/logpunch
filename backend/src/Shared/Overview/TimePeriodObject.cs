namespace Shared;

public class TimePeriodObject
{
    public List<GroupByObject> GroupByObjects { get; set; }

    public TimePeriodObject(List<GroupByObject> groupByObjects)
    {
        GroupByObjects = groupByObjects;
    }
}
