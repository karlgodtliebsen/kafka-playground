﻿using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses;

public class QueryStreamHeader
{
  [JsonPropertyName("queryId")]
  public string QueryId { get; set; }
    
  [JsonPropertyName("columnNames")]
  public string[] ColumnNames { get; set; }
    
  [JsonPropertyName("columnTypes")]
  public string[] ColumnTypes { get; set; }
}