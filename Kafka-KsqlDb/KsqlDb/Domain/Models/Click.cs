namespace KsqlDb.Domain.Models;

public class Click
{
    public string IP_ADDRESS { get; set; } = null!;
    public string URL { get; set; } = null!;
    public string TIMESTAMP { get; set; } = null!;
}