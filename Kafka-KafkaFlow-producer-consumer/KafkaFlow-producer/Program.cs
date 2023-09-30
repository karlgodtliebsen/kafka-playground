using KafkaFlow.Producers;

using KafkaFlow_Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Producer.Configuration;

using Testcontainers.Kafka;

using KafkaConfiguration = Producer.Configuration.KafkaConfiguration;

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.json");
IConfigurationRoot configuration = configurationBuilder.Build();

//https://github.com/Farfetch/kafkaflow/blob/master/samples/KafkaFlow.Sample/Program.cs
KafkaContainer? kafkaContainer = null;
//kafkaContainer = new KafkaBuilder().Build();
if (kafkaContainer is not null)
{
    //add to use testContainer
    await kafkaContainer.StartAsync();
    var kafkaPort = kafkaContainer.GetBootstrapAddress();
    configuration["Kafka:Broker"] = kafkaPort;
}


IHostBuilder builder = new HostBuilder();
builder.ConfigureServices((services) =>
    {
        services.AddProducer(configuration);

    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConsole();
        logging.AddDebug();
    });


var host = builder.Build();
var provider = host.Services;
var config = provider.GetRequiredService<IOptions<KafkaConfiguration>>().Value;

var count = 10000;
Console.WriteLine($"Starting Producer with {count} messages");

var producer = provider.GetRequiredService<IProducerAccessor>().GetProducer(config.ProducerName);
for (var i = 0; i < count; i++)
{
    var id = Ulid.NewUlid().ToString();
    await producer.ProduceAsync(config.Topic, id, new TestMessage { Text = $"Message: {id}" });
    Console.WriteLine($"Produced: message with Id {id}");

    if (i % 100 == 0)
    {
        Task.Delay(1000).Wait();
    }
}


Console.WriteLine("Press <Enter> to stop");
Console.ReadLine();
await Task.Delay(3000);


if (kafkaContainer is not null)
{
    await kafkaContainer.DisposeAsync();
}