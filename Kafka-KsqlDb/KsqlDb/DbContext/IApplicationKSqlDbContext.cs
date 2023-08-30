using ksqlDB.RestApi.Client.KSql.Query.Context;
using KsqlDb.Domain.Models;

namespace KsqlDb.DbContext;

public interface IApplicationKSqlDbContext : IKSqlDBContext
{
    ksqlDB.RestApi.Client.KSql.Linq.IQbservable<Tweet> Tweets { get; }
}