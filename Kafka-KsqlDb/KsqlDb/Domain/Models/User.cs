using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query;

namespace KsqlDb.Domain.Models;

public class User : Record
{
    [JsonPropertyName("ID")]
    public string Id { get; set; }

    [JsonPropertyName("USERID")]
    public string UserId  { get; set; } = null!;
    [JsonPropertyName("GENDER")]

    public string Gender { get; set; }

    [JsonPropertyName("REGIONID")]
    public string RegionId { get; set; }
}