using KsqlDb.Configuration;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddKafka(builder.Configuration);
builder.Services.AddKsqlDb(builder.Configuration);

IHost host = builder.Build();
using (host)
{
    Console.WriteLine("Press any key to exit");
    await host.RunAsync();
    Console.ReadLine();
}