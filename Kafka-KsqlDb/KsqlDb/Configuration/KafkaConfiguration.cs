using Confluent.Kafka;

namespace KsqlDb.Configuration;

public class KafkaConfiguration : ClientConfig
{
    public const string SectionName = "Kafka";
    
    public string? ApplicationId { get; set; } = default!;

    public string BootstrapServers { get; set; } = default!;

}
