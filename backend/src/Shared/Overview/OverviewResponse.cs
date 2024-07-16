namespace Shared
{
    public class OverviewResponse
    {
        public string Type { get; set; }
        public string QueryString { get; set; }

        public TimeModePeriodObject TimeModePeriodObject { get; set; }

        public TimePeriodObject TimePeriodObject { get; set; }

        public OverviewResponse(string type, string queryString, TimeModePeriodObject timeModePeriodObject, TimePeriodObject timePeriodObject)
        {
            Type = type;
            QueryString = queryString;
            TimeModePeriodObject = timeModePeriodObject;
            TimePeriodObject = timePeriodObject;
        }
    }
}
