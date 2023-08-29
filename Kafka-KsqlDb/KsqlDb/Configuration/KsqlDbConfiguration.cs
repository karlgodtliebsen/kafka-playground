using Confluent.Kafka;

namespace KsqlDb.Configuration;

public class KsqlDbConfiguration : ClientConfig
{
    public const string SectionName = "KsqlDb";

    public string? EndPoint { get; set; } = default!;

    public string? Topic { get; set; } = default!;

}
