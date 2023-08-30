using KsqlDb.Domain;
using Microsoft.Extensions.Hosting;

namespace KsqlDb.HostServices;

public class KsqlDbProcessorService : BackgroundService
{
    private readonly KsqlDbTweetProcessor ksqlDbTweetProcessor;

    public KsqlDbProcessorService(KsqlDbTweetProcessor ksqlDbTweetProcessor)
    {
        this.ksqlDbTweetProcessor = ksqlDbTweetProcessor;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ksqlDbTweetProcessor.Run();
    }
}