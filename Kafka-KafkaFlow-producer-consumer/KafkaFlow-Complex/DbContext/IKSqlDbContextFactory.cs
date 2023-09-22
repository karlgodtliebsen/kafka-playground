using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace KsqlDb.DbContext;

public interface IKSqlDbContextFactory
{
    //IApplicationKSqlDbContext Create(string ksqlDbUrl);
    IApplicationKSqlDbContext Create(Action<KSqlDBContextOptions>? options = null);
}