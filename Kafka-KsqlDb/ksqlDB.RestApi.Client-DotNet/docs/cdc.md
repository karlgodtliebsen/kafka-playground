# Change data capture

### Push notifications from SQL Server tables using Kafka, utilizing Change Data Capture (CDC)
You can utilize the `SqlServer.Connector` package and the `Debezium` connector streaming platform to capture **Create**, **Update**, and **Delete** (CUD) operations that occur at the row level in tables,
This process is known as **Change Data Capture** (CDC) and it is performed in this case on SQL Server databases.

### Nuget
```
Install-Package SqlServer.Connector -Version 1.0.0
Install-Package ksqlDB.RestApi.Client
```

[SqlServer.Connector README](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/SqlServer.Connector/README.md)

Full example is available in [Blazor example](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/Blazor.Sample) - InsideOut.sln: (It may take a few minutes for the initial run to complete and for all containers to become operational.)

The following example demonstrates ksqldb server side filtering of database transactions: 
```C#
using System;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using SqlServer.Connector.Cdc;

class Program
{
  static string connectionString = @"Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

  static string bootstrapServers = "localhost:29092";
  static string KsqlDbUrl => @"http://localhost:8088";

  static string tableName = "Sensors";
  static string schemaName = "dbo";

  private static ISqlServerCdcClient CdcClient { get; set; }

  static async Task Main(string[] args)
  {
    CdcClient = new CdcClient(connectionString);

    await CreateSensorsCdcStreamAsync();

    await TryEnableCdcAsync(); //see full example https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/SqlServer.Connector/README.md

    await CreateConnectorAsync(); //see full example

    await using var context = new KSqlDBContext(KsqlDbUrl);

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    var cdcSubscription = context.CreateQuery<IoTSensorChange>("sqlserversensors")
      .WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Where(c => c.Op != "r" && (c.After == null || c.After.SensorId != "d542a2b3-c"))
      .Take(5)
      .ToObservable()
      .Subscribe(cdc =>
        {
          var operationType = cdc.OperationType;
          Console.WriteLine(operationType);

          switch (operationType)
          {
            case ChangeDataCaptureType.Created:
              Console.WriteLine($"Value: {cdc.After.Value}");
              break;
            case ChangeDataCaptureType.Updated:

              Console.WriteLine($"Value before: {cdc.Before.Value}");
              Console.WriteLine($"Value after: {cdc.After.Value}");
              break;
            case ChangeDataCaptureType.Deleted:
              Console.WriteLine($"Value: {cdc.Before.Value}");
              break;
          }
        }, onError: error =>
        {
          semaphoreSlim.Release();

          Console.WriteLine($"Exception: {error.Message}");
        },
        onCompleted: () =>
        {
          semaphoreSlim.Release();
          Console.WriteLine("Completed");
        });


    await semaphoreSlim.WaitAsync();

    using (cdcSubscription)
    {
    }
  }

  private static async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
  {
    string fromName = "sqlserversensors";
    string kafkaTopic = "sqlserver2019.dbo.Sensors";

    var ksqlDbUrl = "http://localhost:8088";

    var httpClient = new HttpClient
    {
      BaseAddress = new Uri(ksqlDbUrl)
    };

    var httpClientFactory = new HttpClientFactory(httpClient);

    var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

    EntityCreationMetadata metadata = new()
    {
      EntityName = fromName,
      KafkaTopic = kafkaTopic,
      ValueFormat = SerializationFormats.Json,
      Partitions = 1,
      Replicas = 1
    };

    var createTypeResponse = await restApiClient.CreateTypeAsync<IoTSensor>(cancellationToken);
    createTypeResponse = await restApiClient.CreateTypeAsync<IoTSensorChange>(cancellationToken);

    var httpResponseMessage = await restApiClient.CreateStreamAsync<DatabaseChangeObject<IoTSensor>>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
      .ConfigureAwait(false);
  }
}
```

`DatabaseChangeObject<TEntity>`:
Represents a generic database change object.
It takes a generic type parameter TEntity, which represents the entity type associated with the database change.

`DatabaseChangeObject<TEntity>` contains the following properties:
- **Before**: Represents the state of the entity before the change occurred.
- **After**: Represents the state of the entity after the change occurred.
- **Op**: Represents the operation performed on the database (e.g., "insert," "update," or "delete").

```C#
public record IoTSensorChange : DatabaseChangeObject<IoTSensor>
{
}

public record IoTSensor
{
  [Key]
  public string SensorId { get; set; }
  public int Value { get; set; }
}
```

The provided C# code defines a set of record classes for handling database change objects in an IoT sensor context.
