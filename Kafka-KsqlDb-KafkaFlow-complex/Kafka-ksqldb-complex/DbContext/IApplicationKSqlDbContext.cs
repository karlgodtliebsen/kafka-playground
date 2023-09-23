using ksqlDB.RestApi.Client.KSql.Query.Context;

using Messages;

namespace KsqlDb.DbContext;

public interface IApplicationKSqlDbContext : IKSqlDBContext
{
    ksqlDB.RestApi.Client.KSql.Linq.IQbservable<DataSourceMessage> Messages { get; }


    //ksqlDB.RestApi.Client.KSql.Linq.IQbservable<IdentityMapMessage> IdentityMap { get; }
}