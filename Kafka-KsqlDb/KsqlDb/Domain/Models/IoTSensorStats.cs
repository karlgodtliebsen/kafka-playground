namespace KsqlDb.Domain.Models;

public record IoTSensorStats
{
    public string SensorId { get; set; } = null!;
    public double AvgValue { get; set; }

    public long WindowStart { get; set; }
    public long WindowEnd { get; set; }
}