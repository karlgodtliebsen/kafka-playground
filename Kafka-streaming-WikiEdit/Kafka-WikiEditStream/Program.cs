using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using WikiEditStream.Configuration;

//https://github.com/confluentinc/confluent-kafka-dotnet/
//https://github.com/confluentinc/WikiEdits/blob/master/Program.cs

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddKafka(builder.Configuration);
IHost host = builder.Build();
using (host)
{
    var configuration = host.Services.GetRequiredService<IOptions<KafkaConfiguration>>().Value;
    //var bootstrap = host.Services.GetRequiredService<BootstrapTestContainer>();
    //await bootstrap.Start();
    //var admin = host.Services.GetRequiredService<KafkaAdminClient>();
    //await admin.DeleteTopic(configuration.TopicSource);
    //await admin.DeleteTopic(configuration.TopicDestination);
    //await admin.CreateTopic(configuration.TopicSource);
    //await admin.CreateTopic(configuration.TopicDestination);
    //await bootstrap.Stop();
}

builder = Host.CreateApplicationBuilder(args);
builder.Services.AddKafka(builder.Configuration);

//
builder.Services.AddKafkaProducerHosts(builder.Configuration);
//builder.Services.AddKafkaConsumerHosts(builder.Configuration);
//builder.Services.AddKafkaStreamingHosts(builder.Configuration);


host = builder.Build();
using (host)
{
    Console.WriteLine("Press any key to exit");
    await host.RunAsync();
    Console.ReadLine();
}