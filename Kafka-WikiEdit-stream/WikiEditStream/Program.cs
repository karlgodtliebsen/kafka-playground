using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using WikiEditStream;
using WikiEditStream.Configuration;

//https://github.com/confluentinc/confluent-kafka-dotnet/
//https://github.com/confluentinc/WikiEdits/blob/master/Program.cs

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddKafka(builder.Configuration);

IHost host = builder.Build();
using (host)
{
    const string topic = "recent_changes";
    var docker = host.Services.GetRequiredService<ConfluentKafkaDockerComposeBuilder>();
    await docker.BuildConfluentDocker();
    Console.WriteLine(docker.PortData());

    var admin = host.Services.GetRequiredService<KafkaAdminClient>();
    await admin.CreateTopic(topic);

    var producer = host.Services.GetRequiredService<KafkaProducer>();
    await producer.Produce(topic);

    var consumer = host.Services.GetRequiredService<KafkaConsumer>();
    consumer.Consume(topic);

    Console.WriteLine("Press any key to exit");
    Console.ReadLine();
    await docker.DisposeAsync();
}



