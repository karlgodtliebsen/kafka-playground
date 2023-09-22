using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace Messages;

public class IdentityMapMessage
{
    [Key]
    public string Id { get; set; }

    public string? Id1 { get; set; }
    public string? Id2 { get; set; }
    public string? Id3 { get; set; }
    public string? Id4 { get; set; }

    public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.UtcNow;

}