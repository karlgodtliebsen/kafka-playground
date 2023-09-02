using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Extensions.Http;
using Blazor.Sample.Kafka;
using InsideOut.Consumer;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.AspNetCore.Components;

namespace Blazor.Sample.Pages;

partial class KafkaStreamComponent : IDisposable
{
  [Inject]
  private IConfiguration Configuration { get; init; }

  [Inject]
  private IKafkaConsumer<string, SensorsStream> ItemsConsumer { get; init; }

  private IDisposable topicSubscription;
    
  private readonly CancellationTokenSource cancellationTokenSource = new();
  
  private string KsqlDbUrl => Configuration[ConfigKeys.KSqlDb_Url];

  protected override async Task OnInitializedAsync()
  {
    await CreateItemsStreamAsync();

    var synchronizationContext = SynchronizationContext.Current;

    // await SubscribeToQuery(synchronizationContext);

    SubscribeToSensors(synchronizationContext);

    await base.OnInitializedAsync();
  }

  private async Task CreateItemsStreamAsync()
  {
    //!!! disclaimer - these steps shouldn't be part of a component initialization. It is intended only for demonstration purposes, to see the relevant parts together.
    await using var context = new KSqlDBContext(KsqlDbUrl);

    var statement = context.CreateOrReplaceStreamStatement(streamName: TopicNames.SensorsStream)
      .As<IoTSensor>()
      .Select(c => new { c.SensorId, c.Value })
      .PartitionBy(c => c.SensorId);

    var httpResponseMessage = await statement.ExecuteStatementAsync(cancellationTokenSource.Token);
      
    var statementResponses = httpResponseMessage.ConvertToStatementResponses();
    //!!! disclaimer
  }

  private void SubscribeToSensors(SynchronizationContext synchronizationContext)
  {
    topicSubscription = ItemsConsumer.ConnectToTopic()
      .ToObservable()
      .Select(c => c.Message)
      .SubscribeOn(NewThreadScheduler.Default)
      .ObserveOn(synchronizationContext)
      .Subscribe(c =>
      {
        c.Value.Id = c.Key;

        items.Enqueue(c.Value);

        StateHasChanged();
      }, error => { Console.WriteLine(error.Message); });
  }

  private async Task SubscribeToQuery(SynchronizationContext synchronizationContext)
  {
    var options = new KSqlDBContextOptions(KsqlDbUrl)
    {
      ShouldPluralizeFromItemName = false
    };

    await using var context = new KSqlDBContext(options);

    context.CreateQuery<SensorsStream>("SensorsStream")
      .ToObservable()
      .ObserveOn(synchronizationContext)
      .Subscribe(c =>
      {
        c.Id = c.Id;

        items.Enqueue(c);
        StateHasChanged();
      }, error => { });
  }

  public void Dispose()
  {
    cancellationTokenSource.Cancel();
    cancellationTokenSource.Dispose();

    topicSubscription?.Dispose();

    ItemsConsumer.Dispose();
  }
}
