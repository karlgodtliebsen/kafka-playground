namespace Producer.Configuration;

public class KafkaConfiguration
{
    public const string SectionName = "Kafka";
    public string? Broker { get; set; } = default!;
    public string Topic { get; set; } = default!;
    public string GroupId { get; set; } = default!;
    public string ProducerName { get; set; } = default!;

}