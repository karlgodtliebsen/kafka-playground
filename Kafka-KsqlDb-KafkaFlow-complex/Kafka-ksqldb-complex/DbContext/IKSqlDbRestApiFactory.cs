using ksqlDB.RestApi.Client.KSql.RestApi;

namespace KsqlDb.DbContext;

public interface IKSqlDbRestApiFactory
{
    IKSqlDbRestApiClient Create();
}