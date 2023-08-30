using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using KsqlDb.Configuration;
using KsqlDb.Domain;
using Microsoft.Extensions.Logging;

//https://github.com/confluentinc/confluent-kafka-dotnet/
//https://github.com/confluentinc/WikiEdits/blob/master/Program.cs

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddKafka(builder.Configuration);
IHost host = builder.Build();
using (host)
{
    var configuration = host.Services.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
    var admin = host.Services.GetRequiredService<KafkaAdminClient>();
    await admin.DeleteTopic(configuration.KafkaTopic);
    await admin.CreateTopic(configuration.KafkaTopic);
}

builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddKafka(builder.Configuration);
builder.Services.AddKsqlDb(builder.Configuration);

//builder.Services.AddKafkaProducerHosts(builder.Configuration);
//builder.Services.AddKafkaConsumerHosts(builder.Configuration);
//builder.Services.AddKafkaStreamingHosts(builder.Configuration);


host = builder.Build();
using (host)
{
    //await using var context = host.Services.GetRequiredService<IApplicationKSqlDbContext>();
    //using var disposable = LatestByOffset(context);
  Console.WriteLine("Press any key to exit");
    await host.RunAsync();
    Console.ReadLine();
}