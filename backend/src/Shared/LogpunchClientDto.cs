namespace Shared;

public class LogpunchClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public LogpunchClientDto()
    {
        Name = "";
    }

}
