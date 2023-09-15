using Confluent.Kafka;

using Microsoft.Extensions.Configuration;

var configFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "getting-started.properties"));
if (!File.Exists(configFile))
{
    Console.WriteLine($"Failed to find file: {configFile}");
    return 0;
}

// var conf = new ProducerConfig { BootstrapServers = "localhost:9092" };
IConfiguration configuration = new ConfigurationBuilder()
    .AddIniFile(configFile)
    .Build();

configuration["group.id"] = "kafka-dotnet-getting-started";
configuration["auto.offset.reset"] = "earliest";

const string topic = "purchases";

CancellationTokenSource cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // prevent the process from terminating.
    cts.Cancel();
};

using var consumer = new ConsumerBuilder<string, string>(configuration.AsEnumerable()).Build();
consumer.Subscribe(topic);
try
{
    while (true)
    {
        var cr = consumer.Consume(cts.Token);
        Console.WriteLine($"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");
    }
}
catch (OperationCanceledException)
{
    // Ctrl-C was pressed.
    Console.WriteLine("Closing Consumer");
}
finally
{
    consumer.Close();
}
return 0;