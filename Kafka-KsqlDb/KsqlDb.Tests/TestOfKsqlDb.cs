using KsqlDb.Domain;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Org.BouncyCastle.Asn1;
using Xunit.Abstractions;
using System.Net.Http;
using FluentAssertions;
using KsqlDb.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace KsqlDbTests;

//https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet
public class TestOfKsqlDb
{
    private readonly ITestOutputHelper output;

    private IHost host;
    private IServiceProvider serviceProvider;
    private readonly KafkaConfiguration kafkaOptions;
    private readonly KsqlDbConfiguration ksqlDbOptions;    

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
        });

        host = builder.Build();
        serviceProvider = host.Services;
        ksqlDbOptions = serviceProvider.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
        kafkaOptions = serviceProvider.GetRequiredService<IOptions<KafkaConfiguration>>().Value;
    }

    [Fact]
    public async Task VerifySetup()
    {
        ksqlDbOptions.EndPoint.Should().Be("http://localhost:8088");
        ksqlDbOptions.Topic.Should().Be("tweet");
    }


    [Fact]
    public async Task TestOfConfluentKafkaSetup()
    {
        var contextOptions = new KSqlDBContextOptions(ksqlDbOptions.EndPoint)
        {
            ShouldPluralizeFromItemName = true
        };

        await using var context = new KSqlDBContext(contextOptions);

        using var subscription = context.CreateQueryStream<Tweet>()
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Where(p => p.Message != "Hello world" || p.Id == 1)
            .Select(l => new { l.Message, l.Id })
            .Take(2)
            .Subscribe(tweetMessage =>
            {
                output.WriteLine($"{nameof(Tweet)}: {tweetMessage.Id} - {tweetMessage.Message}");
            }, error => { output.WriteLine($"Exception: {error.Message}"); }, () => output.WriteLine("Completed"));

        output.WriteLine("done");
    }


    [Fact]
    public async Task TestOfStreamCreation()
    {
        EntityCreationMetadata metadata = new()
        {
            KafkaTopic = ksqlDbOptions.Topic,
            Partitions = 3,
            Replicas = 3
        };

        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(ksqlDbOptions.EndPoint!)
        };

        var httpClientFactory = new HttpClientFactory(httpClient);
        var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

        var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata);
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task TestOfStreamCreationUsingAdminClient()
    {
        var client = serviceProvider.GetRequiredService<KsqlDbAdminClient>();
        await client.CreateStream();
    }


    [Fact]
    public async Task TestOfInsertRecord()
    {
        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(ksqlDbOptions.EndPoint!)
        };

        var httpClientFactory = new HttpClientFactory(httpClient);

        var httpResponseMessage = await new KSqlDbRestApiClient(httpClientFactory)
            .InsertIntoAsync(
                new Tweet { Id = 1, Message = "Hello world" }
            );
        httpResponseMessage.EnsureSuccessStatusCode();

        httpResponseMessage = await new KSqlDbRestApiClient(httpClientFactory)
            .InsertIntoAsync(
                new Tweet { Id = 2, Message = "ksqlDB rulez!" }
            );
        httpResponseMessage.EnsureSuccessStatusCode();
    }


    [Fact]
    public async Task TestOfInsertRecordUsingKSqlDBContext()
    {
        await using var context = new KSqlDBContext(ksqlDbOptions.EndPoint!);

        context.Add(new Tweet { Id = 1, Message = "Hello world" });
        context.Add(new Tweet { Id = 2, Message = "ksqlDB rulez!" });
        var saveChangesResponse = await context.SaveChangesAsync();
    }


}