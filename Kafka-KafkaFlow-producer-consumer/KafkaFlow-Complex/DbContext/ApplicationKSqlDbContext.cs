using ksqlDB.RestApi.Client.KSql.Query.Context;

using Messages;

using Microsoft.Extensions.Logging;

namespace KsqlDb.DbContext;

internal class ApplicationKSqlDbContext : KSqlDBContext, IApplicationKSqlDbContext
{
    public ApplicationKSqlDbContext(string ksqlDbUrl, ILoggerFactory? loggerFactory = null)
        : base(ksqlDbUrl, loggerFactory)
    {
    }

    public ApplicationKSqlDbContext(KSqlDBContextOptions contextOptions, ILoggerFactory? loggerFactory = null)
        : base(contextOptions, loggerFactory)
    {
    }

    public ksqlDB.RestApi.Client.KSql.Linq.IQbservable<DataSourceMessage> Messages => CreateQueryStream<DataSourceMessage>();
}