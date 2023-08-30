using KsqlDb.Domain;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Xunit.Abstractions;
using FluentAssertions;
using KsqlDb.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KsqlDb.DbContext;
using KsqlDb.Domain.Models;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace KsqlDbTests;

//https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet
public class KafkaTestOffUserKsqlDb
{
    private readonly ITestOutputHelper output;
    private readonly IHost host;
    private readonly IServiceProvider serviceProvider;
    public KafkaTestOffUserKsqlDb(ITestOutputHelper output)
    {
        this.output = output;

        var configurationBuilder = new ConfigurationBuilder();//
        configurationBuilder.AddJsonFile("appsettings.IntegrationTests.json");
        IConfigurationRoot configuration = configurationBuilder.Build();

        IHostBuilder builder = new HostBuilder();
        builder.ConfigureServices((services) =>
        {
            services.AddKafka(configuration);
            services.AddKsqlDb(configuration);

        })
        .ConfigureLogging((hostingContext, logging) =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        host = builder.Build();
        serviceProvider = host.Services;
    }


    [Fact]
    public void VerifySetup()
    {
        var ksqlDbOptions = serviceProvider.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
        ksqlDbOptions.EndPoint.Should().Be("http://localhost:8088");
        ksqlDbOptions.KafkaTopic.Should().Be("tweet");
    }
    [Fact]
    public void VerifyConfiguration()
    {
        serviceProvider.GetService<IOptions<KsqlDbConfiguration>>().Should().NotBeNull();
        serviceProvider.GetService<IApplicationKSqlDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IKSqlDbContextFactory>().Should().NotBeNull();
        serviceProvider.GetService<KsqlDbTweetProcessor>().Should().NotBeNull();

        serviceProvider.GetService<IHttpClientFactory>().Should().NotBeNull();
        serviceProvider.GetService<ILoggerFactory>().Should().NotBeNull();
        serviceProvider.GetService<IKSqlDbRestApiClient>().Should().NotBeNull();
    }

    [Fact]
    public async Task TestOfUserStreamSubscription()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();

        await using var context = factory.Create((options => options.ShouldPluralizeFromItemName = true));
        using var subscription = context.CreateQueryStream<User>()
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Where(p => p.Gender != "Hello world")
            .Select(l => new { l.UserId, l.Id, l.Gender, l.RegionId })
            .Take(2)
            .Subscribe(message =>
                {
                    output.WriteLine($"{nameof(User)}: {message.Id} - {message.UserId} - {message.Gender} - {message.RegionId}");
                },
                error => { output.WriteLine($"Exception: {error.Message}"); },
                () => output.WriteLine("Completed"));

        await Task.Delay(10000);
        output.WriteLine("done");
    }
    
    //select * from USERS_TABLE EMIT CHANGES;
    

}
