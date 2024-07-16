namespace Shared
{
    public class ThenByObject
    {
        public string Name { get; set; }
        public int? Total { get; set; }

        // Constructor without ThenByObjects
        public ThenByObject(string name, int? total)
        {
            Name = name;
            Total = total;
        }
    }
}
