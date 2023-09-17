using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;
using FluentAssertions;

using KsqlDb.Domain;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using KsqlDb.Configuration;
using KsqlDb.DbContext;
using KsqlDb.Domain.Models;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;


namespace KsqlDbTests;

//https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet
public class TestOfKafkaksqlDbPageViews
{
    private readonly ITestOutputHelper output;
    private readonly IHost host;
    private readonly IServiceProvider serviceProvider;
    public TestOfKafkaksqlDbPageViews(ITestOutputHelper output)
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
        ksqlDbOptions.KafkaTopic.Should().Be("tweets");
    }

    [Fact]
    public void VerifyConfiguration()
    {
        serviceProvider.GetService<IOptions<KsqlDbConfiguration>>().Should().NotBeNull();
        serviceProvider.GetService<IApplicationKSqlDbContext>().Should().NotBeNull();
        serviceProvider.GetService<IKSqlDbContextFactory>().Should().NotBeNull();
        serviceProvider.GetService<KsqlDbTweetProcessor>().Should().NotBeNull();

        serviceProvider.GetService<ILoggerFactory>().Should().NotBeNull();
        serviceProvider.GetService<IHttpClientFactory>().Should().NotBeNull();
        serviceProvider.GetService<IKSqlDbRestApiClient>().Should().NotBeNull();
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


    [Fact]
    public async Task TestOfKsqlDbStreamCreationUsingConfiguredServices()
    {
        var ksqlDbOptions = serviceProvider.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
        output.WriteLine(ksqlDbOptions.KafkaTopic);
        var metadata = new EntityCreationMetadata()
        {
            KafkaTopic = ksqlDbOptions.KafkaTopic!.ToLowerInvariant(),
            Partitions = 1,
            Replicas = 1,
            EntityName = nameof(Tweet),
            ShouldPluralizeEntityName = true
        };
        var restApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();

        var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata, CancellationToken.None);
        output.WriteLine(httpResponseMessage.ReasonPhrase);
        httpResponseMessage.EnsureSuccessStatusCode();
    }



    [Fact]
    public async Task TestOfInsertRecord()
    {
        var restApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();
        var insertProperties = new InsertProperties()
        {
            EntityName = nameof(Tweet),
            ShouldPluralizeEntityName = true,
        };
        for (int i = 0; i < 100; i++)
        {
            var httpResponseMessage = await restApiClient.InsertIntoAsync(new Tweet { Id = 4, Message = "Hello world" + i.ToString() }, insertProperties, cancellationToken: CancellationToken.None);
            output.WriteLine(httpResponseMessage.ReasonPhrase);
            httpResponseMessage.EnsureSuccessStatusCode();

            httpResponseMessage = await restApiClient.InsertIntoAsync(new Tweet { Id = 2, Message = "ksqlDB rulez!" + i.ToString() }, insertProperties, cancellationToken: CancellationToken.None);
            output.WriteLine(httpResponseMessage.ReasonPhrase);
            httpResponseMessage.EnsureSuccessStatusCode();
            await Task.Delay(1);
        }
    }


    [Fact]
    public async Task TestOfInsertRecordsUsingKSqlDBContext()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();

        await using var context = factory.Create(options =>
        {
            options.ShouldPluralizeFromItemName = true;
        });

        int limit = 100;
        for (int i = 0; i < limit; i++)
        {
            context.Add(new Tweet { Id = limit + i, Message = "Hello world" });
            context.Add(new Tweet { Id = limit * 2 + i, Message = "ksqlDB rulez!" });
        }

        var saveChangesResponse = await context.SaveChangesAsync(cancellationToken: CancellationToken.None);

        output.WriteLine(saveChangesResponse.ReasonPhrase);
        saveChangesResponse.EnsureSuccessStatusCode();
        saveChangesResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task TestOfInsertRecordsWithSameIdUsingKSqlDBContext()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();

        await using var context = factory.Create(options =>
        {
            options.ShouldPluralizeFromItemName = true;
        });


        int id1 = 42424242;
        int id2 = 43434343;

        int limit = 100;
        for (int i = 0; i < limit; i++)
        {
            var mid1 = Ulid.NewUlid().ToString();
            var mid2 = Ulid.NewUlid().ToString();
            context.Add(new Tweet { Id = id1, Message = $"Hello world   - {mid1}" });
            context.Add(new Tweet { Id = id2, Message = $"ksqlDB rulez! - {mid2}" });
        }

        var saveChangesResponse = await context.SaveChangesAsync(cancellationToken: CancellationToken.None);

        output.WriteLine(saveChangesResponse.ReasonPhrase);
        saveChangesResponse.EnsureSuccessStatusCode();
        saveChangesResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task TestOfAddRecordUsingKSqlDBContext()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();
        var insertProperties = new InsertProperties()
        {
            EntityName = nameof(Tweet),
            ShouldPluralizeEntityName = true,
        };
        await using var context = factory.Create(options =>
        {
            options.ShouldPluralizeFromItemName = true;
        });

        var tweet1 = new Tweet { Id = 100, Message = "Hello world" };
        var tweet2 = new Tweet { Id = 200, Message = "ksqlDB rulez!" };
        context.Add(tweet1, insertProperties);
        context.Add(tweet2, insertProperties);

    }

    [Fact]
    public async Task TestOfTweetStreamSubscription()
    {
        var factory = serviceProvider.GetRequiredService<IKSqlDbContextFactory>();
        await using var context = factory.Create((options =>
        {
            options.ShouldPluralizeFromItemName = true;
        }));

        using var subscription = context.CreateQueryStream<Tweet>()
            .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
            .Select(l => new { l.Message, l.Id, l.Amount })
            .Take(2)
            .Subscribe(message =>
                {
                    output.WriteLine($"{nameof(Tweet)}: {message.Id} - {message.Message} - {message.Amount}");
                },
                error => { output.WriteLine($"Exception: {error.Message}"); },
                () => output.WriteLine("Completed")
                );
        await Task.Delay(5000);
        output.WriteLine("done");
    }

}
