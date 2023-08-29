using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;

using Xunit.Abstractions;

namespace WikiEditStream.Tests;

public class Tweet : Record
{
    public int Id { get; set; }

    public string Message { get; set; }
}


//https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet
public class TestOfKsqlDb
{
    private readonly ITestOutputHelper output;

    public TestOfKsqlDb(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task TestOfConfluentKafkaSetup()
    {
        var ksqlDbUrl = @"http://localhost:8088";

        var contextOptions = new KSqlDBContextOptions(ksqlDbUrl)
        {
            ShouldPluralizeFromItemName = true
        };

        await using var context = new KSqlDBContext(contextOptions);

        using var subscription = context.CreateQueryStream<Tweet>()
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Where(p => p.Message != "Hello world" || p.Id == 1)
            .Select(l => new { l.Message, l.Id })
            .Take(2)
            .Subscribe(tweetMessage =>
            {
                output.WriteLine($"{nameof(Tweet)}: {tweetMessage.Id} - {tweetMessage.Message}");
            }, error => { output.WriteLine($"Exception: {error.Message}"); }, () => output.WriteLine("Completed"));

        output.WriteLine("done");


    }

}