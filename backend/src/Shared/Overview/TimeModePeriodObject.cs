namespace Shared;

public class TimeModePeriodObject
{
    public string Name { get; set; }
    public string Timespan { get; set; }
    public int? Total { get; set; }

    public TimeModePeriodObject(string name, string timespan, int? total)
    {
        Name = name;
        Timespan = timespan;
        Total = total;
    }
}
