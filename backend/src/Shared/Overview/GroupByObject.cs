namespace Shared
{
    public class GroupByObject
    {
        public string Name { get; set; }
        public int? Total { get; set; }
        public List<ThenByObject> ThenByObjects { get; set; }

        // Constructor without ThenByObjects
        public GroupByObject(string name, int? total)
        {
            Name = name;
            Total = total;
            ThenByObjects = [];
        }

        // Constructor with ThenByObjects
        public GroupByObject(string name, int? total, List<ThenByObject> thenByObjects)
        {
            Name = name;
            Total = total;
            ThenByObjects = thenByObjects;
        }
    }
}
