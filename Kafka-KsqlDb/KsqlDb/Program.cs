using KsqlDb.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.json");
IConfigurationRoot configuration = configurationBuilder.Build();

IHostBuilder builder = Host.CreateDefaultBuilder();
builder.ConfigureServices((services) =>
    {
        services.AddKafka(configuration);
        services.AddKsqlDb(configuration);

    })
    .ConfigureLogging((_, logging) =>
    {
        logging.AddConsole();
        logging.AddDebug();
    });

IHost host = builder.Build();
using (host)
{
    Console.WriteLine("Starting Host. Press <Enter> to exit");
    await host.RunAsync();
    Console.ReadLine();
}