using Confluent.Kafka;
using Confluent.Kafka.Admin;

using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

using KsqlDb.Configuration;

using Microsoft.Extensions.Options;

namespace KsqlDb.Domain;


//https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/ksqlDb.RestApi.Client/KSql/RestApi/Statements/StatementTemplates.cs#L20

public class KsqlDbAdminClient
{
    private readonly IKSqlDbRestApiClient restApiClient;
    private readonly KsqlDbConfiguration config;

    public KsqlDbAdminClient(IKSqlDbRestApiClient restApiClient, IOptions<KsqlDbConfiguration> options)
    {
        this.restApiClient = restApiClient;
        config = options.Value;
    }

    public async Task CreateTopic(string topicName, CancellationToken cancellationToken = default)
    {

        var adminConfig = new AdminClientConfig(config);
        using var adminClient = new AdminClientBuilder(adminConfig).Build();
        await adminClient.CreateTopicsAsync(new TopicSpecification[]
        {
            new TopicSpecification
            {
                Name = topicName,
                NumPartitions = 1,
                ReplicationFactor = 1
            }
        });
    }

    public async Task CreateStream<T>(string? kafkaTopic = null, string? tableName = null, CancellationToken cancellationToken = default)
    {
        var name = (tableName ?? nameof(T)).ToLowerInvariant();
        var topic = (kafkaTopic ?? name).ToLowerInvariant();
        var metadata = new EntityCreationMetadata()
        {
            KafkaTopic = topic,
            ValueFormat = SerializationFormats.Json,
            Partitions = 1,
            Replicas = 1,
            EntityName = name,
            ShouldPluralizeEntityName = true
        };

        var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<T>(metadata, cancellationToken: cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public async Task CreateType<T>(CancellationToken cancellationToken)
    {
        var httpResponseMessage = await restApiClient.CreateTypeAsync<T>(cancellationToken: cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public async Task CreateTable<T>(string kafkaTopic, string? tableName = null, CancellationToken cancellationToken = default)
    {
        var name = (tableName ?? nameof(T)).ToLowerInvariant();
        var topic = kafkaTopic.ToLowerInvariant();
        var metadata = new EntityCreationMetadata()
        {
            KafkaTopic = topic,
            ValueFormat = SerializationFormats.Json,
            Partitions = 1,
            Replicas = 1,
            EntityName = name,
            ShouldPluralizeEntityName = true,
        };
        var httpResponseMessage = await restApiClient.CreateOrReplaceTableAsync<T>(metadata, cancellationToken: cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    //https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/statements.md
    //https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/5e02510d3fd48108f3724c8cc2e25b48101d78dc/docs/streams_and_tables.md
    public async Task CreateSourceTable<T>(string kafkaTopic, string? tableName = null, CancellationToken cancellationToken = default)
    {
        var name = (tableName ?? nameof(T)).ToLowerInvariant();
        var topic = kafkaTopic.ToLowerInvariant();
        var metadata = new EntityCreationMetadata()
        {
            KafkaTopic = topic,
            ValueFormat = SerializationFormats.Json,
            Partitions = 1,
            Replicas = 1,
            EntityName = name,
            ShouldPluralizeEntityName = true,
        };
        var httpResponseMessage = await restApiClient.CreateSourceTableAsync<T>(metadata, ifNotExists: true, cancellationToken: cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public async Task CreateQuerableTable(string tableName, CancellationToken cancellationToken = default)
    {
        //CREATE TABLE QUERYABLE_TOPICIDENTITYTABLES AS SELECT * FROM TOPICIDENTITYTABLES;

        string statement = $"CREATE TABLE QUERYABLE_{tableName}  AS SELECT * FROM {tableName};";
        var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(statement));
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public async Task CreateTable(string? kafkaTopic, string tableName, bool deleteTopic = true, CancellationToken cancellationToken = default)
    {
        string statement =
    $"""
        CREATE OR REPLACE TABLE {tableName.ToLowerInvariant()}(
            Id VARCHAR PRIMARY KEY,
            Id1 VARCHAR,
            Id2 VARCHAR,
            Id3 VARCHAR,
            Id4 VARCHAR,
            DateTimeOffset TIMESTAMP
        ) WITH(KAFKA_TOPIC = '{kafkaTopic.ToLowerInvariant()}', VALUE_FORMAT = 'Json', PARTITIONS = '1', REPLICAS = '1');
    """;

        var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"{statement}"));
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public async Task DropStream<T>(string? kafkaTopic = null, bool deleteTopic = true, CancellationToken cancellationToken = default)
    {
        var name = (kafkaTopic ?? nameof(T)).ToLowerInvariant();
        string deleteTopicClause = deleteTopic ? " DELETE TOPIC" : string.Empty;
        var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"DROP STREAM IF EXISTS {name} {deleteTopicClause};"));
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public async Task DropType<T>(string? typeName = null, CancellationToken cancellationToken = default)
    {
        var name = (typeName ?? nameof(T)).ToLowerInvariant();
        var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"DROP TYPE IF EXISTS {name};"));
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public async Task DropTable<T>(string? tableName = null, bool deleteTopic = false, CancellationToken cancellationToken = default)
    {
        var name = (tableName ?? nameof(T)).ToLowerInvariant();
        string deleteTopicClause = deleteTopic ? " DELETE TOPIC" : string.Empty;
        var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"DROP TABLE IF EXISTS {name} {deleteTopicClause};"));
        httpResponseMessage.EnsureSuccessStatusCode();
    }


}