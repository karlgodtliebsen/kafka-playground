
using Confluent.Kafka;

using Microsoft.Extensions.Configuration;


var configFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, "getting-started.properties"));
if (!File.Exists(configFile))
{
    Console.WriteLine($"Failed to find file: {configFile}");
    return 0;
}

IConfiguration configuration = new ConfigurationBuilder()
    .AddIniFile(configFile)
    .Build();

const string topic = "purchases";

string[] users = { "eabara", "jsmith", "sgarcia", "jbernard", "htanaka", "awalther" };
string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };

//var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
using var producer = new ProducerBuilder<string, string>(configuration.AsEnumerable()).Build();

var numProduced = 0;
var rnd = new Random();
const int numMessages = 10000;
for (int i = 0; i < numMessages; ++i)
{
    var user = users[rnd.Next(users.Length)];
    var item = items[rnd.Next(items.Length)];

    producer.Produce(topic, new Message<string, string> { Key = user, Value = item },
        (deliveryReport) =>
        {
            if (deliveryReport.Error.Code != ErrorCode.NoError)
            {
                Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
            }
            else
            {
                Console.WriteLine($"Produced event to topic {topic}: key = {user,-10} value = {item}");
                numProduced += 1;
            }
        });

    if (i % 100 == 0)
    {
        producer.Flush(TimeSpan.FromSeconds(10));
        Task.Delay(1000).Wait();
    }
}

producer.Flush(TimeSpan.FromSeconds(10));
Console.WriteLine($"{numProduced} messages were produced to topic {topic}");
return 0;