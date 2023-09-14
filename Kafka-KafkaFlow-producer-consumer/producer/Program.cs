using KafkaFlow;
using KafkaFlow.Producers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Producer.Configuration;

using Testcontainers.Kafka;

using KafkaConfiguration = Producer.Configuration.KafkaConfiguration;


//https://github.com/Farfetch/kafkaflow/blob/master/samples/KafkaFlow.Sample/Program.cs

var kafkaContainer = new KafkaBuilder().Build();
await kafkaContainer.StartAsync();
var kafkaPort = kafkaContainer.GetBootstrapAddress();


var configurationBuilder = new ConfigurationBuilder();//
configurationBuilder.AddJsonFile("appsettings.IntegrationTests.json");
IConfigurationRoot configuration = configurationBuilder.Build();


var v = configuration["Kafka:Broker"];
configuration["Kafka:Broker"] = kafkaPort;


IHostBuilder builder = new HostBuilder();
builder.ConfigureServices((services) =>
    {
        services.AddKafka(configuration);

    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConsole();
        logging.AddDebug();
    });


var host = builder.Build();
var provider = host.Services;
var config = provider.GetRequiredService<IOptions<KafkaConfiguration>>().Value;

var bus = provider.CreateKafkaBus();
await bus.StartAsync();


var count = 10;
Console.WriteLine($"Starting Producer with {count} messages");

var producer = provider.GetRequiredService<IProducerAccessor>().GetProducer(config.ProducerName);
for (var i = 0; i < count; i++)
{
    var id = Ulid.NewUlid().ToString(); //var id = Guid.NewGuid().ToString();
    await producer.ProduceAsync(config.Topic, id, new TestMessage { Text = $"Message: {id}" });
    Console.WriteLine($"Produced: message with Id {id}");
}



Console.WriteLine("Press <Enter> to stop");
Console.ReadLine();
await Task.Delay(3000);


await kafkaContainer.DisposeAsync();