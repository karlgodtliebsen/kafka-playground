using KsqlDb.Configuration;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.Extensions.Options;

namespace KsqlDb.Domain;

public class KsqlDbAdminClient
{
    private readonly KsqlDbConfiguration config;

    public KsqlDbAdminClient(IOptions<KsqlDbConfiguration> options)
    {
        config = options.Value;
    }


    public async Task CreateStream()
    {

        EntityCreationMetadata metadata = new()
        {
            KafkaTopic = config.Topic,
            Partitions = 3,
            Replicas = 3
        };

        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(config.EndPoint!)
        };

        var httpClientFactory = new HttpClientFactory(httpClient);
        var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

        var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata);
        httpResponseMessage.EnsureSuccessStatusCode();
    }


}