using FlowControl.Configuration;

using KafkaFlow;

using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;

using KsqlDb.Configuration;
using KsqlDb.DbContext;

using Messages;

using Microsoft.Extensions.Options;

namespace KsqlDb.Domain;


public sealed class KsqlDbDataSourceProcessor : IAsyncDisposable
{
    private readonly KafkaFlowConfiguration flowOptions;
    private readonly IServiceProvider provider;
    private readonly IKSqlDbContextFactory factory;
    private readonly KsqlDbAdminClient client;
    private readonly KsqlDbConfiguration ksqlDbOptions;

    //const string IdentityMapTopicName = "TopicIdentityMap";
    //const string DataSourceMessageTopicName = "TopicDataSource";
    //const string OutboundMessageTopicName = "TopicOutbound";
    const string IdentityMapTableName = "TOPICIDENTITYTABLES";
    const string IdentityMapMaterializedViewName = "QUERYABLE_TOPICIDENTITYTABLE";


    public KsqlDbDataSourceProcessor(IServiceProvider provider, IOptions<KsqlDbConfiguration> ksqlDbOptions, IOptions<KafkaFlowConfiguration> flowOptions, IKSqlDbContextFactory factory, KsqlDbAdminClient client)
    {
        this.provider = provider;
        this.factory = factory;
        this.client = client;
        this.flowOptions = flowOptions.Value;
        this.ksqlDbOptions = ksqlDbOptions.Value;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var bus = KafkaFlow.ServiceProviderExtensions.CreateKafkaBus(provider);
        try
        {
            await bus.StartAsync(cancellationToken);
            var producer = bus.Producers[flowOptions.OutboundProducerName];

            await using var context = factory.Create((options => options.ShouldPluralizeFromItemName = true));
            await ProcessInboundMessages(context, producer, cancellationToken);
        }
        finally
        {
            await bus.StopAsync();
        }
    }


    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private async Task<IdentityMapMessage?> FindIdentityMap(IApplicationKSqlDbContext context, string id, CancellationToken cancellationToken)
    {
        //await client.DropTable<IdentityMapMessage>(IdentityMapMaterializedViewName, cancellationToken: cancellationToken);
        //await client.DropTable<IdentityMapMessage>(IdentityMapTableName, cancellationToken: cancellationToken);
        //await client.CreateTable<IdentityMapMessage>(flowOptions.IdentityMapTopic, IdentityMapTableName, cancellationToken);
        //await client.CreateQuerableTable(IdentityMapTableName, cancellationToken: CancellationToken.None);

        var pullQuery = context.CreatePullQuery<IdentityMapMessage>(IdentityMapMaterializedViewName)
            .Where(c => c.Id == id)
            .Take(1);

        var result = await pullQuery.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    private async Task ProcessInboundMessages(IApplicationKSqlDbContext context, IMessageProducer messageProducer, CancellationToken cancellationToken)
    {
        // await client.CreateStream<DataSourceMessage>(flowOptions.DataSourceTopic, cancellationToken: CancellationToken.None);

        context.Messages
            //.Select(g => new { Id = g.Key, Earliest = g.EarliestByOffset(c => c.Message) })
            //.Select(g => new { Id = g.Key, Earliest = g.EarliestByOffsetAllowNulls(c => c.Message) })
            //.Select(g => new { Id = g.Key, Earliest = g.LatestByOffset(c => c.Message) })
            .Select(l => l)
            .Take(2) // LIMIT 2    
            .Subscribe(onNext: inboundMessage =>
            {
                Console.WriteLine($"{nameof(Messages)}: {inboundMessage}");
                Console.WriteLine();
            }, onError: error => { Console.WriteLine($"Exception: {error.Message}"); }, onCompleted: () => Console.WriteLine("Completed"));

        //using var subscription = context.CreateQueryStream<DataSourceMessage>()
        //    .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
        //    .Select(l => l)
        //    .Take(2)
        //    .Subscribe(
        //        async inboundMessage =>
        //    {
        //        Console.WriteLine("inboundMessage");
        //        Console.WriteLine(inboundMessage.ToJson());

        //        var identityMap = await FindIdentityMap(context, inboundMessage.Id, cancellationToken);
        //        Console.WriteLine("identityMap");
        //        Console.WriteLine(identityMap.ToJson());

        //        var outboundMessage = new OutboundMessage()
        //        {
        //            Id = inboundMessage.Id,
        //            IdentityMap = identityMap
        //        };

        //        Console.WriteLine("outboundMessage");
        //        Console.WriteLine(outboundMessage.ToJson());
        //        //produce output message
        //        //use producer to OutBoundTopic
        //        var deliveryResult = await messageProducer.ProduceAsync(outboundMessage.Id, outboundMessage);
        //        //verify result else error handling

        //    }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));

    }

}