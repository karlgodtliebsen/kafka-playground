using KsqlDb.Configuration;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KsqlDb.DbContext;

internal class KSqlDbRestApiFactory : IKSqlDbRestApiFactory
{
    private readonly ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory clientFactory;
    private readonly ILoggerFactory loggerFactory;
    private readonly KsqlDbConfiguration contextOptions;

    public KSqlDbRestApiFactory(ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory clientFactory,ILoggerFactory loggerFactory, IOptions<KsqlDbConfiguration> contextOptions)
    {
        this.clientFactory = clientFactory;
        this.loggerFactory = loggerFactory;
        this.contextOptions = contextOptions.Value;
    }

    public IKSqlDbRestApiClient Create()
    {
        var client = new KSqlDbRestApiClient(clientFactory, loggerFactory);
        return client;
    }
}
