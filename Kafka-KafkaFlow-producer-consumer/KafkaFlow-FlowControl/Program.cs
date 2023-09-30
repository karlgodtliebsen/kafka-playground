using FlowControl.Configuration;

using KafkaFlow;

using KafkaFlow_Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Testcontainers.Kafka;

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.IntegrationTests.json");
IConfigurationRoot configuration = configurationBuilder.Build();

//https://github.com/Farfetch/kafkaflow/blob/master/samples/KafkaFlow.Sample/Program.cs
KafkaContainer? kafkaContainer = null;
// kafkaContainer = new KafkaBuilder().Build();
if (kafkaContainer is not null)    //use testContainer
{
    await kafkaContainer.StartAsync();
    var kafkaPort = kafkaContainer.GetBootstrapAddress();
    configuration["Kafka:Broker"] = kafkaPort;
}


IHostBuilder builder = new HostBuilder();
builder.ConfigureServices((services) =>
    {
        services.AddFlowControl(configuration);

    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConsole();
        logging.AddDebug();
    });


var host = builder.Build();
var provider = host.Services;
var config = provider.GetRequiredService<IOptions<KafkaFlowConfiguration>>().Value;

var bus = provider.CreateKafkaBus();

await bus.StartAsync();

var producer = bus.Producers[config.ProducerName];
var consumer = bus.Consumers[config.ConsumerName];
var run = true;
while (run)
{
    //if (run)
    {
        Console.Write("Number of messages to produce, Start, Stop, Pause, Resume, or Exit: ");
        var input = Console.ReadLine()?.ToLower();

        switch (input)
        {
            case var _ when int.TryParse(input, out var count):
                for (var i = 0; i < count; i++)
                {
                    var id = Ulid.NewUlid().ToString();
                    var message = new SampleMessage
                    {
                        Id = id,
                        Name = $"Name_{id}"
                    };

                    producer.Produce(message.Id, message);
                }

                break;

            case "pause":
                consumer.Pause(consumer.Assignment);
                Console.WriteLine("Consumer paused");

                break;

            case "resume":
                consumer.Resume(consumer.Assignment);
                Console.WriteLine("Consumer resumed");
                break;

            case "stop":
                await consumer.StopAsync();
                Console.WriteLine("Consumer stopped");

                break;

            case "start":
                await consumer.StartAsync();
                Console.WriteLine("Consumer started");

                break;

            case "exit":
                await bus.StopAsync();
                run = false;
                break;
        }
    }
}

Console.WriteLine("Stopping...");

if (kafkaContainer is not null)
{
    await Task.Delay(1000);
    await kafkaContainer.DisposeAsync();
}