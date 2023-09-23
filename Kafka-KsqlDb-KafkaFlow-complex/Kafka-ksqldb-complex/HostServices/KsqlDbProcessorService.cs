using KsqlDb.Domain;

using Microsoft.Extensions.Hosting;

namespace KsqlDb.HostServices;

public class KsqlDbProcessorService : BackgroundService
{
    private readonly KsqlDbDataSourceProcessor ksqlDbTweetProcessor;

    public KsqlDbProcessorService(KsqlDbDataSourceProcessor ksqlDbTweetProcessor)
    {
        this.ksqlDbTweetProcessor = ksqlDbTweetProcessor;
    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ksqlDbTweetProcessor.Run(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await ksqlDbTweetProcessor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}