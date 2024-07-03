namespace Shared;

public class OverviewResponse(
    string queryString,
    TimeModePeriodObject timeModePeriodObject,
    TimePeriodObject timePeriodObject)
{
    public string QueryString { get; set; } = queryString;


    public TimeModePeriodObject TimeModePeriodObject { get; set; } = timeModePeriodObject;

    public TimePeriodObject TimePeriodObject { get; set; } = timePeriodObject;
}
