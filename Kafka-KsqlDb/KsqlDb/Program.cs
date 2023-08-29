using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using KsqlDb.Configuration;
using KsqlDb.Domain;

//https://github.com/confluentinc/confluent-kafka-dotnet/
//https://github.com/confluentinc/WikiEdits/blob/master/Program.cs

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddKafka(builder.Configuration);
IHost host = builder.Build();
using (host)
{
    var configuration = host.Services.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
    var admin = host.Services.GetRequiredService<KafkaAdminClient>();
    await admin.DeleteTopic(configuration.Topic);
    await admin.CreateTopic(configuration.Topic);
}

builder = Host.CreateApplicationBuilder(args);
builder.Services.AddKafka(builder.Configuration);
builder.Services.AddKsqlDb(builder.Configuration);

//builder.Services.AddKafkaProducerHosts(builder.Configuration);
//builder.Services.AddKafkaConsumerHosts(builder.Configuration);
//builder.Services.AddKafkaStreamingHosts(builder.Configuration);


host = builder.Build();
using (host)
{
    Console.WriteLine("Press any key to exit");
    await host.RunAsync();
    Console.ReadLine();
}