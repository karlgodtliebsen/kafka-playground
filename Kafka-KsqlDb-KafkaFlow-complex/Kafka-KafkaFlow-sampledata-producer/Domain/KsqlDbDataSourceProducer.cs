using FlowControl.Configuration;

using KafkaFlow;

using Messages;

using Microsoft.Extensions.Options;

namespace KsqlDb.Domain;


public sealed class KsqlDbDataSourceProducer
{

    private readonly IServiceProvider provider;
    private readonly KafkaFlowConfiguration config;
    private static int counter = 0;
    public KsqlDbDataSourceProducer(IServiceProvider provider, IOptions<KafkaFlowConfiguration> options)
    {
        this.provider = provider;
        this.config = options.Value;
    }
    public async Task Run(CancellationToken cancellationToken)
    {

        Console.WriteLine("Starting identity mapping data production to IdentityMaæ....");
        const int count = 10000;
        IList<string> identities = new List<string>(count * 2);
        var bus = provider.CreateKafkaBus();

        await bus.StartAsync(cancellationToken);
        var producerIdentityMap = bus.Producers[config.IdentityMapProducerName];
        var producerDataSource = bus.Producers[config.DataSourceProducerName];
        for (int i = 0; i < count; i++)
        {
            var id = i.ToString();
            identities.Add(id);
            var message = new IdentityMapMessage
            {
                Id = id,
                Id1 = $"Id1_{id}-" + counter,
                Id2 = $"Id2_{id}-" + counter,
                Id3 = $"Id3_{id}-" + counter,
                Id4 = $"Id4_{id}-" + counter,
            };
            producerIdentityMap.Produce(message.Id, message);
        }

        for (int i = 0; i < count; i++)
        {
            var id = Ulid.NewUlid().ToString();
            identities.Add(id);
            var message = new IdentityMapMessage
            {
                Id = id,
                Id1 = $"Id1_{id}-" + counter,
                Id2 = $"Id2_{id}-" + counter,
                Id3 = $"Id3_{id}-" + counter,
                Id4 = $"Id4_{id}-" + counter,
            };
            producerIdentityMap.Produce(message.Id, message);
        }

        Console.WriteLine("Starting influx of test data to DataSource...");
        try
        {
            while (true)
            {
                foreach (var id in identities)
                {
                    var message = new DataSourceMessage
                    {
                        Id = id,
                        Name = "Name " + counter,
                        Prop1 = "Prop1 " + counter,
                        Prop2 = "Prop2 " + counter,
                        Prop3 = "Prop3 " + counter,
                        Prop4 = "Prop4 " + counter,
                    };
                    producerDataSource.Produce(message.Id, message);
                }
                counter++;
                Task.Delay(10000).Wait();
            }
        }
        finally
        {
            await bus.StopAsync();
        }
    }

}