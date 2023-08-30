using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

using KsqlDb.Configuration;
using KsqlDb.Domain.Models;

using Microsoft.Extensions.Options;

namespace KsqlDb.Domain;

public class KsqlDbAdminClient
{
    private readonly IKSqlDbRestApiClient restApiClient;
    private readonly KsqlDbConfiguration config;

    public KsqlDbAdminClient(IKSqlDbRestApiClient restApiClient, IOptions<KsqlDbConfiguration> options)
    {
        this.restApiClient = restApiClient;
        config = options.Value;
    }

    public async Task CreateStream(CancellationToken cancellationToken)
    {
        var metadata = new EntityCreationMetadata()
        {
            KafkaTopic = config.KafkaTopic!.ToLowerInvariant(),
            Partitions = 1, //TOBEFIXED
            Replicas = 1, //TOBEFIXED
            EntityName = nameof(Tweet),
            ShouldPluralizeEntityName = true
        };

        var httpResponseMessage =
            await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata, cancellationToken: cancellationToken);
        httpResponseMessage.EnsureSuccessStatusCode();
    }
}