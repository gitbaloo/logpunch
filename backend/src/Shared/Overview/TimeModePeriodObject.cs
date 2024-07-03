namespace Shared;

public class TimeModePeriodObject
{
    public string Name { get; set; }
    public string Timespan { get; set; }
    public double Total { get; set; }

    public TimeModePeriodObject(string name, string timespan, double total)
    {
        Name = name;
        Timespan = timespan;
        Total = total;
    }
}
