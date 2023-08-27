using Confluent.Kafka;

namespace WikiEditStream.Configuration;

public class KafkaConfiguration: ClientConfig
{
    public const string SectionName = "Kafka";
}