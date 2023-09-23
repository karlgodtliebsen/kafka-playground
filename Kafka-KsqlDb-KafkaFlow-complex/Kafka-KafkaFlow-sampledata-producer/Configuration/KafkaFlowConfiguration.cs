namespace FlowControl.Configuration;

public class KafkaFlowConfiguration
{
    public const string SectionName = "KafkaFlow";
    public string? Broker { get; set; } = default!;

    public string DataSourceTopic { get; set; } = default!;
    public string IdentityMapTopic { get; set; } = default!;

    public string GroupId { get; set; } = default!;

    public string IdentityMapProducerName { get; set; } = default!;

    public string DataSourceProducerName { get; set; } = default!;

}