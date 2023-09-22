using Confluent.Kafka;

namespace KsqlDb.Configuration;

public class KsqlDbConfiguration : ClientConfig
{
    public const string SectionName = "KsqlDb";

    public string? EndPoint { get; set; } = default!;

    public string? ClusterName { get; set; } = default!;
    public string? ClusterId { get; set; } = default!;
}
