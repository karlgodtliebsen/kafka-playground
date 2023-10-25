using KsqlDb.Domain;

using Microsoft.Extensions.Hosting;

namespace KsqlDb.HostServices;

public class ProducerService : BackgroundService
{
    private readonly DataSourceProducer producer;

    public ProducerService(DataSourceProducer producer)
    {
        this.producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Starting Sample Data Production");
        await producer.Run(stoppingToken);
    }

}