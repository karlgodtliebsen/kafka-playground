using FlowControl.Configuration;

using KsqlDb.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.json");
IConfigurationRoot configuration = configurationBuilder.Build();


IHostBuilder builder = new HostBuilder();
builder.ConfigureServices((services) =>
    {
        services
            .AddFlowControl(configuration)
            .AddKsqlDb(configuration);
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConsole();
        logging.AddDebug();
    });


var host = builder.Build();
using (host)
{
    Console.WriteLine("Starting ksqlDb Processor Host. Press <Enter> to exit");
    await host.RunAsync();
    Console.ReadLine();
}

Console.WriteLine("Stopping...");
