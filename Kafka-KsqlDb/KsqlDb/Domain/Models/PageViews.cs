using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query;

namespace KsqlDb.Domain.Models;

public class PageViews : Record
{
    [JsonPropertyName("userid")]
    public string UserId { get; set; }

    [JsonPropertyName("viewtime")]
    public long ViewTime { get; set; }

    [JsonPropertyName("partition")]
    public long Partition { get; set; } 

    [JsonPropertyName("offset")]
    public long Offset { get; set; }

    [JsonPropertyName("pageid")]
    public string PageId { get; set; }
}