using KsqlDb.Configuration;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KsqlDb.DbContext;

internal class KSqlDbContextFactory : IKSqlDbContextFactory
{
    private readonly ILoggerFactory loggerFactory;
    private readonly KsqlDbConfiguration contextOptions;

    public KSqlDbContextFactory(ILoggerFactory loggerFactory, IOptions<KsqlDbConfiguration> contextOptions)
    {
        this.loggerFactory = loggerFactory;
        this.contextOptions = contextOptions.Value;
    }

    public IApplicationKSqlDbContext Create(string ksqlDbUrl)
    {
        return new ApplicationKSqlDbContext(ksqlDbUrl, loggerFactory);
    }
    
    public IApplicationKSqlDbContext Create(Action<KSqlDBContextOptions>? options = null)
    {
        var ctxOptions = new KSqlDBContextOptions(contextOptions.EndPoint);
        options?.Invoke(ctxOptions);
        return new ApplicationKSqlDbContext(ctxOptions, loggerFactory);
    }
}