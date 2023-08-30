using KsqlDb.Domain;
using Microsoft.Extensions.Hosting;

namespace KsqlDb.HostServices;

public class KsqlDbProcessorService : BackgroundService
{
    private readonly KsqlDbProcessor ksqlDbProcessor;

    public KsqlDbProcessorService(KsqlDbProcessor ksqlDbProcessor)
    {
        this.ksqlDbProcessor = ksqlDbProcessor;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ksqlDbProcessor.Run();
    }
}