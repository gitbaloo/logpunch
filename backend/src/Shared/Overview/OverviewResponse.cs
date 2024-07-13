namespace Shared;

public class OverviewResponse
{
    public string QueryString { get; set; }

    public TimeModePeriodObject TimeModePeriodObject { get; set; }

    public TimePeriodObject TimePeriodObject { get; set; }

    public OverviewResponse(string queryString, TimeModePeriodObject timeModePeriodObject, TimePeriodObject timePeriodObject)
    {
        QueryString = queryString;
        TimeModePeriodObject = timeModePeriodObject;
        TimePeriodObject = timePeriodObject;
    }
}
