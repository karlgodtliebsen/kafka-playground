using ksqlDB.RestApi.Client.KSql.Query;
using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace KsqlDb.Domain.Models;


public class Tweet : Record
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    
    [JsonPropertyName("MESSAGE")]
    public string Message { get; set; } = null!;


    [JsonPropertyName("TEST")]
    public string Test { get; set; } = "test"!;

    [JsonPropertyName("AMOUNT")] 
    public double Amount { get; set; } = 42.0;

    //[JsonPropertyName("ACCOUNTBALANCE")]
    //[Decimal(3, 2)]
    //public decimal AccountBalance { get; set; } = 1.0m;
}
