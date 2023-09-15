using Consumer.Configuration;

using KafkaFlow;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Testcontainers.Kafka;

using KafkaConfiguration = Consumer.Configuration.KafkaConfiguration;


//https://github.com/Farfetch/kafkaflow/blob/master/samples/KafkaFlow.Sample/Program.cs

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.IntegrationTests.json");
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
        services.AddConsumer(configuration);

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


Console.WriteLine("Press <Enter> to stop");
Console.ReadLine();
await Task.Delay(3000);


if (kafkaContainer is not null)
{
    await kafkaContainer.DisposeAsync();
}