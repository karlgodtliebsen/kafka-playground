namespace FlowControl.Configuration;

public class KafkaFlowConfiguration
{
    public const string SectionName = "KafkaFlow";
    public string? Broker { get; set; } = default!;
    public string Topic { get; set; } = default!;
    public string GroupId { get; set; } = default!;
    public string ProducerName { get; set; } = default!;

    public string ConsumerName { get; set; } = default!;

}