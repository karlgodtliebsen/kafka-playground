# SqlServer.Connector
The `SqlServer.Connector` is a .NET client API that enables the consumption of row-level table changes, specifically utilizing **Change Data Capture** (CDC) functionality, from SQL Server databases. This API works seamlessly with the `Debezium` connector streaming platform.

**Kafka Connect** is an open-source component of the Apache Kafka ecosystem that provides a scalable and reliable framework for connecting external systems with Kafka. It allows you to easily integrate data from various sources and sinks into Kafka, enabling efficient and real-time data pipelines.

A **Kafka Connector** is a plug-in or module that extends the functionality of Kafka Connect. It provides the integration logic necessary to connect external systems or applications with Apache Kafka.
Connectors enable the ingestion of data from external sources into Kafka (source connectors) or the delivery of data from Kafka to external sinks (sink connectors).

### Blazor Sample 
Set `docker-compose.csproj` as startup project in `InsideOut.sln` for an embedded Kafka connect integration.

The initial run takes a few minutes until all containers are up and running.

### Nuget
```
Install-Package SqlServer.Connector -Version 1.0.0

Install-Package ksqlDb.RestApi.Client
```

# v1.0.0
KsqlDbConnect's constructor argument was changed from an Uri to `ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory.IHttpClientFactory`

```
var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);

var restApiClient = new KsqlDbConnect(httpClientFactory);
```

### Subscribing to a CDC stream (v0.1.0)
The Debezium connector produces change events from row-level table changes into a Kafka topic.
The following program shows how to subscribe to such streams with `ksqlDB`.

```C#
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDb.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.KSql.RestApi;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Cdc.Connectors;
using SqlServer.Connector.Cdc.Extensions;

class Program
{
  static string connectionString = @"Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

  static string bootstrapServers = "localhost:29092";
  static string KsqlDbUrl => @"http://localhost:8088";

  static string tableName = "Sensors";
  static string schemaName = "dbo";

  static async Task Main(string[] args)
  {
    await CreateSensorsCdcStreamAsync();

    await TryEnableCdcAsync();

    await CreateConnectorAsync();

    await using var context = new KSqlDBContext(KsqlDbUrl);

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    var cdcSubscription = context.CreateQuery<RawDatabaseChangeObject<IoTSensor>>("sqlserversensors")
      .WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Take(5)
      .ToObservable()
      .Subscribe(cdc =>
        {
          var operationType = cdc.OperationType;
          Console.WriteLine(operationType);

          switch (operationType)
          {
            case ChangeDataCaptureType.Created:
              Console.WriteLine($"Value: {cdc.EntityAfter.Value}");
              break;
            case ChangeDataCaptureType.Updated:

              Console.WriteLine($"Value before: {cdc.EntityBefore.Value}");
              Console.WriteLine($"Value after: {cdc.EntityAfter.Value}");
              break;
            case ChangeDataCaptureType.Deleted:
              Console.WriteLine($"Value: {cdc.EntityBefore.Value}");
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
}

public record IoTSensor
{
	public string SensorId { get; set; }
	public int Value { get; set; }
}
```

Navigate to http://localhost:9000/topic/sqlserver2019.dbo.Sensors for information about the created Kafka topic and view messages with [Kafdrop](https://github.com/obsidiandynamics/kafdrop)

### Creating a CDC stream in ksqldb-cli
TODO: Create stream as select with ksqlDb.RestApi.Client
```KSQL
CREATE STREAM IF NOT EXISTS sqlserversensors (
	op string,
	before STRUCT<SensorId VARCHAR, Value INT>,
	after STRUCT<SensorId VARCHAR, Value INT>,
	source STRUCT<Version VARCHAR, schema VARCHAR, "table" VARCHAR, "connector" VARCHAR>
  ) WITH (
    kafka_topic = 'sqlserver2019.dbo.Sensors',
    value_format = 'JSON'
);

SET 'auto.offset.reset' = 'earliest';
SELECT * FROM sqlserversensors EMIT CHANGES;
```

Sql server DML:
```SQL
INSERT INTO [dbo].[Sensors] ([SensorId], [Value])
VALUES ('734cac20-4', 33);

DELETE FROM [dbo].[Sensors] WHERE [SensorId] = '734cac20-4';

UPDATE [Sensors] SET [Value] = 45 WHERE [SensorId] = '02f8427c-6';
```

Output:
```
+----+-----------------------------------------+---------------------------------------+-----------------------------------------------------------------+
|OP  |BEFORE                                   |AFTER                                  |SOURCE                                                           |
+----+-----------------------------------------+---------------------------------------+-----------------------------------------------------------------+
|c   |null                                     |{SENSORID=734cac20-4, VALUE=33}        |{VERSION=1.5.0.Final, SCHEMA=dbo, table=Sensors, connector=sqlser|
|    |                                         |                                       |ver}                                                             |
|d   |{SENSORID=734cac20-4, VALUE=33}          |null                                   |{VERSION=1.5.0.Final, SCHEMA=dbo, table=Sensors, connector=sqlser|
|    |                                         |                                       |ver}                                                             |
|u   |{SENSORID=02f8427c-6, VALUE=1855}        |{SENSORID=02f8427c-6, VALUE=45}        |{VERSION=1.5.0.Final, SCHEMA=dbo, table=Sensors, connector=sqlser|
|    |                                         |                                       |ver}                                                             |  
```

### Consuming table change events directly from a Kafka topic
```
Install-Package SqlServer.Connector -Version 0.1.0
Install-Package System.Interactive.Async -Version 5.0.0
```

```C#
async Task Main()
{
  string bootstrapServers = "localhost:29092";

  var consumerConfig = new ConsumerConfig
  {
    BootstrapServers = bootstrapServers,
    GroupId = "Client-01",
    AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
  };

  var kafkaConsumer =
    new KafkaConsumer<string, DatabaseChangeObject<IoTSensor>>("sqlserver2019.dbo.Sensors", consumerConfig);

  await foreach (var consumeResult in kafkaConsumer.ConnectToTopic().ToAsyncEnumerable().Take(10))
  {
    Console.WriteLine(consumeResult.Message);
  }

  using (kafkaConsumer)
  {
  }
}
```

### CdcClient (v0.1.0)
CdcEnableDbAsync - Enables change data capture for the current database. 

CdcEnableTableAsync - Enables change data capture for the specified source table in the current database.

```C#
using SqlServer.Connector.Cdc;

private static async Task TryEnableCdcAsync()
{
  var cdcClient = new CdcClient(connectionString);

  try
  {
    await cdcClient.CdcEnableDbAsync();
    
    if(!await CdcProvider.IsCdcTableEnabledAsync(tableName))
      await CdcProvider.CdcEnableTableAsync(tableName);
  }
  catch (Exception e)
  {
    Console.WriteLine(e);
  }
}
```

### IsCdcDbEnabledAsync and IsCdcTableEnabledAsync (v0.2.0)
IsCdcDbEnabledAsync - Has SQL Server database enabled Change Data Capture (CDC). 
IsCdcTableEnabledAsync - Has table Change Data Capture (CDC) enabled on a SQL Server database.

```C#
bool isCdcEnabled = await ClassUnderTest.IsCdcDbEnabledAsync(databaseName);
bool isTableCdcEnabled = await CdcProvider.IsCdcTableEnabledAsync(tableName)
```

### CdcEnableTable (v0.1.0)
SQL parameters for [sys.sp_cdc_enable_table](https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sys-sp-cdc-enable-table-transact-sql?view=sql-server-ver15#arguments)
Default schema name is 'dbo'.

```C#
string tableName = "Sensors";
string captureInstance = $"dbo_{tableName}_v2";

var cdcEnableTable = new CdcEnableTable(tableName)
{
  CaptureInstance = captureInstance,
  IndexName = "IX_Sensors_Name"
};

await CdcProvider.CdcEnableTableAsync(cdcEnableTable);
```

Optional arguments were added in (v0.2.0):
- IndexName, CaptureInstance, CapturedColumnList, FilegroupName

### SqlServerConnectorMetadata, ConnectorMetadata (v0.1.0)
SQL Server connector configuration example:
```C#
using System;
using System.Threading.Tasks;
using SqlServer.Connector.Cdc.Connectors;

private static SqlServerConnectorMetadata CreateConnectorMetadata()
{
  var createConnector = new SqlServerConnectorMetadata(connectionString)
    .SetTableIncludeListPropertyName($"{schemaName}.{tableName}")
    .SetJsonKeyConverter()
    .SetJsonValueConverter()
    .SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
    .SetProperty("database.history.kafka.topic", $"dbhistory.{tableName}")
    .SetProperty("database.server.name", "sqlserver2019")
    .SetProperty("key.converter.schemas.enable", "false")
    .SetProperty("value.converter.schemas.enable", "false")
    .SetProperty("include.schema.changes", "false");

  return createConnector as SqlServerConnectorMetadata;
}
```

### KsqlDbConnect (v.0.1.0)
Creating the connector with `ksqlDB`

```C#
using SqlServer.Connector.Connect;

private static async Task CreateConnectorAsync()
{
  var ksqlDbConnect = new KsqlDbConnect(new Uri(KsqlDbUrl));

  SqlServerConnectorMetadata connectorMetadata = CreateConnectorMetadata();

  await ksqlDbConnect.CreateConnectorAsync(connectorName: "MSSQL_SENSORS_CONNECTOR", connectorMetadata);
}
```

Above mentioned C# snippet is equivavelent to:
```KSQL
CREATE SOURCE CONNECTOR MSSQL_SENSORS_CONNECTOR WITH (
  'connector.class'= 'io.debezium.connector.sqlserver.SqlServerConnector', 
  'database.port'= '1433', 
  'database.hostname'= '127.0.0.1', 
  'database.user'= 'SA', 
  'database.password'= '<YourNewStrong@Passw0rd>', 
  'database.dbname'= 'Sensors', 
  'key.converter'= 'org.apache.kafka.connect.json.JsonConverter', 
  'value.converter'= 'org.apache.kafka.connect.json.JsonConverter', 
  'table.include.list'= 'dbo.Sensors', 
  'database.history.kafka.bootstrap.servers'= 'localhost:29092', 
  'database.history.kafka.topic'= 'dbhistory.Sensors', 
  'database.server.name'= 'sqlserver2019', 
  'key.converter.schemas.enable'= 'false', 
  'value.converter.schemas.enable'= 'false', 
  'include.schema.changes'= 'false'
);
```

### Creating a stream for CDC - Change data capture  (ksqlDb.RestApi.Client)

```C#
using System;
using System.Threading;
using System.Threading.Tasks;
using ksqlDb.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using SqlServer.Connector.Cdc;

private static async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
{
  await using var context = new KSqlDBContext(KsqlDbUrl);

  string fromName = "sqlserversensors";
  string kafkaTopic = "sqlserver2019.dbo.Sensors";

  var httpClientFactory = new HttpClientFactory(new Uri(KsqlDbUrl));

  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  EntityCreationMetadata metadata = new()
  {
    EntityName = fromName,
    KafkaTopic = kafkaTopic,
    ValueFormat = SerializationFormats.Json,
    Partitions = 1,
    Replicas = 1
  };

  var httpResponseMessage = await restApiClient.CreateStreamAsync<RawDatabaseChangeObject>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
    .ConfigureAwait(false);
}
```

### KsqlDbConnect - Drop Connectors (v0.2.0)
DropConnectorAsync - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement fails if the connector doesn't exist.
    
DropConnectorIfExistsAsync - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement doesn't fail if the connector doesn't exist.

```C#
string connectorName = "MSSQL_SENSORS_CONNECTOR";

await ksqlDbConnect.DropConnectorAsync(connectorName);

await ksqlDbConnect.DropConnectorIfExistsAsync(connectorName);
```

### KsqlDbConnect - Get Connectors (v0.2.0)
GetConnectorsAsync - List all connectors in the Connect cluster.

```C#
var ksqlDbUrl = Configuration["ksqlDb:Url"];

var ksqlDbConnect = new KsqlDbConnect(new Uri(ksqlDbUrl));
      
var response = await ksqlDbConnect.GetConnectorsAsync();
```

### Disable CDC from.NET (v0.1.0)
CdcDisableTableAsync - Disables change data capture for the specified source table and capture instance in the current database.
CdcDisableDbAsync - Disables change data capture for the current database.
```C#
var cdcClient = new SqlServer.Connector.Cdc.CdcClient(connectionString);

await cdcClient.CdcDisableTableAsync(tableName, schemaName).ConfigureAwait(false);

await cdcClient.CdcDisableDbAsync().ConfigureAwait(false);
```

### `RawDatabaseChangeObject<TEntity>` (v.0.1.0)
```C#
using System;
using System.Reactive;
using ksqlDb.RestApi.Client.KSql.Query.Context;
using SqlServer.Connector.Cdc;

await using var context = new KSqlDBContext(@"http://localhost:8088");

context.CreateQuery<RawDatabaseChangeObject<IoTSensor>>("sqlserversensors")
  .Subscribe(new AnonymousObserver<RawDatabaseChangeObject<IoTSensor>>(dco =>
  {
    Console.WriteLine($"Operation type: {dco.OperationType}");
    Console.WriteLine($"Before: {dco.Before}");
    Console.WriteLine($"EntityBefore: {dco.EntityBefore?.Value}");
    Console.WriteLine($"After: {dco.After}");
    Console.WriteLine($"EntityAfter: {dco.EntityAfter?.Value}");
    Console.WriteLine($"Source: {dco.Source}");
  }));
```

### KsqlDbConnect (v.0.2.0)
CreateConnectorIfNotExistsAsync - Create a new connector in the Kafka Connect cluster with the configuration passed in the connectorMetadata parameter. The statement does not fail if a connector with the supplied name already exists.

```C#
private static async Task CreateConnectorAsync()
{
  var ksqlDbConnect = new KsqlDbConnect(new Uri(KsqlDbUrl));

  SqlServerConnectorMetadata connectorMetadata = CreateConnectorMetadata();

  connector.connectorMetadata = ConnectorType.Sink;

  await ksqlDbConnect.CreateConnectorIfNotExistsAsync(connectorName: "MSSQL_SENSORS_SINK_CONNECTOR", connectorMetadata);
}

```KSQL
CREATE SINK CONNECTOR IF NOT EXISTS MSSQL_SENSORS_SINK_CONNECTOR ...
```

### ConnectRestApiClient CreateConnectorAsync (v.0.3.0)
- REST API client for managing connectors. 
```C#
using System;
using System.Threading.Tasks;
using ksqlDb.RestApi.Client.KSql.RestApi;
using SqlServer.Connector.Cdc.Connectors;
using SqlServer.Connector.Connect;

static string ConnectUrl => @"http://localhost:8083";

async Task Main()
{
  SqlServerConnectorMetadata connectorMetadata = CreateConnectorMetadata();

  string connectorName = "MSSQL_CDC_CONNECTOR";
	
  var httpClientFactory = new HttpClientFactory(new Uri(ConnectUrl));
	
  var connectRestApiClient = new ConnectRestApiClient(httpClientFactory);
	
  var httpResponseMessage = await connectRestApiClient.CreateConnectorAsync(connectorMetadata, connectorName).Dump();
}
```

### KsqlDb server side query for database transactions
```
Install-Package SqlServer.Connector -Version 0.3.0-rc.1
Install-Package ksqlDb.RestApi.Client -Version 1.0.0
```

The following example demonstrates `ksqlDB` server side filtering of database transactions: 
```C#
using System;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Cdc.Extensions;

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

    await TryEnableCdcAsync();

    await CreateConnectorAsync();

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

    var ksqlDbUrl = Configuration[ConfigKeys.KSqlDb_Url];

    var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

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

### ksqlDB connector info
```KSQL
SHOW CONNECTORS;

DESCRIBE CONNECTOR MSSQL_SENSORS_CONNECTOR;
```

### ksqlDB Cleanup
```KSQL
DROP CONNECTOR MSSQL_SENSORS_CONNECTOR;

DROP STREAM sqlserversensors DELETE TOPIC;
```

### Debezium connector for SQL Server
[Download Debezium connector](https://www.confluent.io/hub/debezium/debezium-connector-sqlserver)

[Deployment](https://debezium.io/documentation/reference/1.5/connectors/sqlserver.html#sqlserver-deploying-a-connector)

### Linqpad
[SqlServer.Connector](https://github.com/tomasfabian/ksqlDb.RestApi.Client/blob/main/Samples/ksqlDb.RestApi.Client.LinqPad/SqlServer.Connector.linq)

### Related sources
[Debezium](https://github.com/debezium/debezium)

[Debezium source connector for SQL server](https://debezium.io/documentation/reference/1.5/connectors/sqlserver.html)

[ksqlDB](https://ksqldb.io/)

# Acknowledgements:
- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/)

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/tomasfabian)
