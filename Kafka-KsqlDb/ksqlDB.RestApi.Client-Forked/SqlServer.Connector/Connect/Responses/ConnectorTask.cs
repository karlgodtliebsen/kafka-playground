﻿using System.Text.Json.Serialization;

namespace SqlServer.Connector.Connect.Responses
{
  /// <summary>
  /// Active task generated by the connector.
  /// </summary>
  public record ConnectorTask
  {
    [JsonPropertyName("connector")]
    public string Connector { get; set; }

    [JsonPropertyName("task")]
    public int Task { get; set; }
  }
}