using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace Messages;

public class OutboundMessage
{
    [Key]
    public string Id { get; set; }

    public string Name { get; set; }

    public string Prop1 { get; set; }
    public string Prop2 { get; set; }
    public string Prop3 { get; set; }
    public string Prop4 { get; set; }

    public IdentityMapMessage IdentityMap { get; set; }
    public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.UtcNow;
}