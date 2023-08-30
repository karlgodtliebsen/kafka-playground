using KsqlDb.Domain;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
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
using System.Net.Http;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace KsqlDbTests;

//https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet
public class TestOfKsqlDb
{
    private readonly ITestOutputHelper output;
    private readonly IHost host;
    private readonly IServiceProvider serviceProvider;
    public TestOfKsqlDb(ITestOutputHelper output)
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
        serviceProvider.GetService<KsqlDbProcessor>().Should().NotBeNull();

        serviceProvider.GetService<IHttpClientFactory>().Should().NotBeNull();
        serviceProvider.GetService<ILoggerFactory>().Should().NotBeNull();
        serviceProvider.GetService<IKSqlDbRestApiClient>().Should().NotBeNull();
    }

    [Fact]
    public async Task TestOfTweetStreamSubscription()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();
        await using var context = factory.Create((options => options.ShouldPluralizeFromItemName = true));
        using var subscription = context.CreateQueryStream<Tweet>()
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Where(p => p.Message != "Hello world" || p.Id == 1)
            .Select(l => new { l.Message, l.Id })
            .Take(2)
            .Subscribe(message =>
            {
                output.WriteLine($"{nameof(Tweet)}: {message.Id} - {message.Message}");
            }, error => { output.WriteLine($"Exception: {error.Message}"); }, () => output.WriteLine("Completed"));
        await Task.Delay(10000);
        output.WriteLine("done");
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

    [Fact]
    public async Task TestOfPageViewsStreamSubscription()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();

        await using var context = factory.Create((options => options.ShouldPluralizeFromItemName = false));
        using var subscription = context.CreateQueryStream<PageViews>("PAGEVIEWS_STREAM")
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Where(p => p.UserId != "Hello world")
            .Select(l => new { l.UserId, l.PageId, l.ViewTime })
            .Take(2)
            .Subscribe(message =>
                {
                    output.WriteLine($"{nameof(PageViews)}: {message.UserId} - {message.UserId} - {message.PageId} - {message.ViewTime}");
                },
                error => { output.WriteLine($"Exception: {error.Message}"); },
                () => output.WriteLine("Completed"));

        await Task.Delay(10000);
        output.WriteLine("done");
    }


    //select * from USERS_TABLE EMIT CHANGES;

    [Fact]
    public async Task TestOfKsqlDbStreamCreation()
    {
        var ksqlDbOptions = serviceProvider.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
        output.WriteLine(ksqlDbOptions.KafkaTopic);
        var metadata = new EntityCreationMetadata()
        {
            KafkaTopic =  ksqlDbOptions.KafkaTopic,
            Partitions = 1,
            Replicas = 1
        };
        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(@"http://localhost:8088")
        };

        var httpClientFactory = new HttpClientFactory(httpClient);
        var restApiClient = new KSqlDbRestApiClient(httpClientFactory);
        var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata);
        output.WriteLine(httpResponseMessage.ReasonPhrase);
        //httpResponseMessage.EnsureSuccessStatusCode();
    }


    [Fact]
    public async Task TestOfKsqlDbStreamCreationUsingConfiguredServices()
    {
        var ksqlDbOptions = serviceProvider.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
        output.WriteLine(ksqlDbOptions.KafkaTopic);
        var metadata = new EntityCreationMetadata()
        {
            KafkaTopic = ksqlDbOptions.KafkaTopic,
            Partitions = 1,
            Replicas = 1
        };
        var restApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();

        var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata, CancellationToken.None);
        output.WriteLine(httpResponseMessage.ReasonPhrase);
        //httpResponseMessage.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TestOfStreamCreationUsingAdminClient()
    {
        var client = serviceProvider.GetRequiredService<KsqlDbAdminClient>();
        await client.CreateStream(CancellationToken.None);
    }


    [Fact]
    public async Task TestOfInsertRecord()
    {
        var restApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();

        var httpResponseMessage = await restApiClient.InsertIntoAsync(new Tweet { Id = 1, Message = "Hello world" }, cancellationToken: CancellationToken.None);
        output.WriteLine(httpResponseMessage.ReasonPhrase);
        httpResponseMessage.EnsureSuccessStatusCode();

        httpResponseMessage = await restApiClient.InsertIntoAsync(new Tweet { Id = 2, Message = "ksqlDB rulez!" }, cancellationToken: CancellationToken.None);
        output.WriteLine(httpResponseMessage.ReasonPhrase);
        httpResponseMessage.EnsureSuccessStatusCode();
    }


    [Fact]
    public async Task TestOfInsertRecordUsingKSqlDBContext()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();

        await using var context = factory.Create((options => options.ShouldPluralizeFromItemName = true));

        context.Add(new Tweet { Id = 1, Message = "Hello world" });
        context.Add(new Tweet { Id = 2, Message = "ksqlDB rulez!" });
        var saveChangesResponse = await context.SaveChangesAsync(cancellationToken: CancellationToken.None);
        output.WriteLine(saveChangesResponse.ReasonPhrase);
        saveChangesResponse.EnsureSuccessStatusCode();
        saveChangesResponse.Should().NotBeNull();
    }


}
