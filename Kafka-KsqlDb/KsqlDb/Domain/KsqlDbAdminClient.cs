using KsqlDb.Configuration;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.Options;
using KsqlDb.Domain.Models;
using System.Threading;

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
        var  metadata = new EntityCreationMetadata()
        {
            KafkaTopic = config.KafkaTopic,
            Partitions = 1,
            Replicas = 1
        };
        var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata, cancellationToken: cancellationToken);
        Console.WriteLine(httpResponseMessage.ReasonPhrase);
        //httpResponseMessage.EnsureSuccessStatusCode();
    }
}