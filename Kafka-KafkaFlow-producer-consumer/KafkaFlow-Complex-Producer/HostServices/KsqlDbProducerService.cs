using KsqlDb.Domain;

using Microsoft.Extensions.Hosting;

namespace KsqlDb.HostServices;

public class KsqlDbProducerService : BackgroundService
{
    private readonly KsqlDbDataSourceProducer producer;

    public KsqlDbProducerService(KsqlDbDataSourceProducer producer)
    {
        this.producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Starting production");
        await producer.Run(stoppingToken);
    }

}