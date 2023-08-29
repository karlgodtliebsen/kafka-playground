using ksqlDB.RestApi.Client.KSql.Query;

namespace KsqlDb.Domain;

public class Tweet : Record
{
    public int Id { get; set; }

    public string Message { get; set; }
}