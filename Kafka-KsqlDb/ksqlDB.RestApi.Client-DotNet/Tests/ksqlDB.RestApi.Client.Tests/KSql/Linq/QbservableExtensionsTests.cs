using FluentAssertions;
using ksqlDB.Api.Client.Tests.KSql.Query.Context;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using UnitTests;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using TestParameters = ksqlDB.Api.Client.Tests.Helpers.TestParameters;

namespace ksqlDB.Api.Client.Tests.KSql.Linq;

public class QbservableExtensionsTests : TestBase
{
  [Test]
  public void SelectConstant_BuildKSql_PrintsConstant()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => "Hello world");

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT 'Hello world' FROM Locations EMIT CHANGES;");
  }

  #region OperatorPrecedence

  [Test]
  public void PlusOperatorPrecedence_BuildKSql_PrintsParentheses()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => (c.Longitude + c.Longitude) * c.Longitude);

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT ({nameof(Location.Longitude)} + {nameof(Location.Longitude)}) * {nameof(Location.Longitude)} FROM Locations EMIT CHANGES;");
  }

  [Test]
  public void OperatorPrecedence_BuildKSql_PrintsParentheses()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => c.Longitude + c.Longitude * c.Longitude);

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT {nameof(Location.Longitude)} + ({nameof(Location.Longitude)} * {nameof(Location.Longitude)}) FROM Locations EMIT CHANGES;");
  }

  [Test]
  public void OperatorPrecedenceTwoAliases_BuildKSql_PrintsParentheses()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { First = c.Longitude / (c.Longitude / 4), Second = c.Longitude / c.Longitude / 5 });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    var expectedKsql = $@"SELECT {nameof(Location.Longitude)} / ({nameof(Location.Longitude)} / 4) AS First, ({nameof(Location.Longitude)} / {nameof(Location.Longitude)}) / 5 AS Second FROM Locations EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }
    
  [Test]
  public void OperatorPrecedenceInWhereClause_NoOrder_BuildKSql_PrintsParentheses()
  {
    //Arrange
    var query = CreateStreamSource()
      .Where(c => c.Latitude == "1" || c.Latitude != "2" && c.Latitude == "3");

    //Act
    var ksql = query.ToQueryString();

    //Assert
    string columnName = nameof(Location.Latitude);

    string expected = @$"SELECT * FROM Locations
WHERE ({columnName} = '1') OR (({columnName} != '2') AND ({columnName} = '3')) EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expected);
  }    

  [Test]
  public void OperatorPrecedenceInWhereClause_BuildKSql_PrintsParentheses()
  {
    //Arrange
    var query = CreateStreamSource()
      .Where(c => (c.Latitude == "1" || c.Latitude != "2") && c.Latitude == "3");

    //Act
    var ksql = query.ToQueryString();

    //Assert
    string columnName = nameof(Location.Latitude);

    ksql.Should().BeEquivalentTo(@$"SELECT * FROM Locations
WHERE (({columnName} = '1') OR ({columnName} != '2')) AND ({columnName} = '3') EMIT CHANGES;");
  }

  #endregion

  [Test]
  public void SelectConstants_BuildKSql_PrintsConstants()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { Message = "Hello world", Age = 23 });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be("SELECT 'Hello world' Message, 23 Age FROM Locations EMIT CHANGES;");
  }

  [Test]
  public void ToQueryString_BuildKSql_PrintsQuery()
  {
    //Arrange
    int limit = 2;

    var query = CreateStreamSource()
      .Take(limit);

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM Locations EMIT CHANGES LIMIT {limit};");
  }

  record MyType
  {
    public int A { get; set; }
    public int B { get; set; }
  }

  [Test]
  public void ToQueryString_TransformWithNestedStruct()
  {
    //Arrange
    var value = new Dictionary<string, MyType>
      { { "a", new MyType { A = 1, B = 2 } } };

    var query = CreateStreamSource()
      .Select(_ => new
      {
        Dict = K.Functions.Transform(value, (k, v) => k.ToUpper(), (k, v) => v.A + 1)
      });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be("SELECT TRANSFORM(MAP('a' := STRUCT(A := 1, B := 2)), (k, v) => UCASE(k), (k, v) => v->A + 1) Dict FROM Locations EMIT CHANGES;");
  }

  [Test]
  public void ToQueryString_CalledTwice_PrintsSameQuery()
  {
    //Arrange
    int limit = 2;

    var query = CreateStreamSource()
      .Take(limit);

    //Act
    var ksql1 = query.ToQueryString();
    var ksql2 = query.ToQueryString();

    //Assert
    ksql1.Should().BeEquivalentTo(ksql2);
  }

  internal class TweetsKQueryStreamSet : KQueryStreamSet<Tweet>
  {
    public TweetsKQueryStreamSet(IServiceScopeFactory serviceScopeFactory, QueryContext queryContext) : base(serviceScopeFactory, queryContext)
    {
    }
  }

  internal class TestableDbProviderExt : TestableDbProvider<Tweet>
  {
    private readonly string ksqlDbUrl;

    public TestableDbProviderExt(string ksqlDbUrl) : base(ksqlDbUrl)
    {
      this.ksqlDbUrl = ksqlDbUrl;

      RegisterKSqlQueryGenerator = false;
    }

    public IQbservable<Tweet> CreateTweetsStreamSet(string? streamName = null)
    {
      var serviceScopeFactory = Initialize(new KSqlDBContextOptions(ksqlDbUrl));

      var queryStreamContext = new QueryContext
      {
        FromItemName = streamName
      };

      return new TweetsKQueryStreamSet(serviceScopeFactory, queryStreamContext);
    }    
  }

  [Test]
  public void ToQueryString_BuildKSqlOnDerivedClass_PrintsQuery()
  {
    //Arrange
    var context = new TestableDbProviderExt(TestParameters.KsqlDBUrl);
    var query = context.CreateTweetsStreamSet();

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM Tweets EMIT CHANGES;");
  }

  [Test]
  public async Task ToAsyncEnumerable_Query_KSqldbProviderRunWasCalled()
  {
    //Arrange
    var context = new TestableDbProvider(TestParameters.KsqlDBUrl);
    context.KSqlDbProviderMock.Setup(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
      .Returns(GetTestValues);
    var query = context.CreateQueryStream<string>();

    //Act
    var asyncEnumerable = query.ToAsyncEnumerable();

    //Assert
    context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.IsAny<QueryStreamParameters>(), It.IsAny<CancellationToken>()), Times.Once);

    await asyncEnumerable.GetAsyncEnumerator().DisposeAsync();
  }

  [Test]
  public async Task ToAsyncEnumerable_Enumerate_ValuesWereReceived()
  {
    //Arrange
    var query = CreateTestableKStreamSet();

    //Act
    var asyncEnumerable = query.ToAsyncEnumerable();

    //Assert
    bool wasValueReceived = false;
    await foreach (var value in asyncEnumerable)
      wasValueReceived = true;

    wasValueReceived.Should().BeTrue();
  }

  [Test]
  public void ToObservable_QueryShouldBeDeferred_KSqlDbProviderRunWasNotCalled()
  {
    //Arrange
    var context = new TestableDbProvider(TestParameters.KsqlDBUrl);
      
    var query = context.CreateQueryStream<string>();

    //Act
    var observable = query.ToObservable();

    //Assert
    observable.Should().NotBeNull();

    context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Test]
  public void ToObservable_DisposeSubscription()
  {
    //Arrange
    CancellationToken cancellationToken = default;

    var context = new TestableDbProvider(TestParameters.KsqlDBUrl);
    context.KSqlDbProviderMock.Setup(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
      .Callback<object, CancellationToken>((par, ct) => { cancellationToken = ct; })
      .Returns(GetTestValues);

    var query = context.CreateQueryStream<string>();

    //Act
    query.ToObservable().Subscribe().Dispose();

    //Assert
    cancellationToken.IsCancellationRequested.Should().BeTrue();
  }

  protected async IAsyncEnumerable<string> GetTestValues()
  {
    yield return "Hello world";

    yield return "Goodbye";

    await Task.CompletedTask;
  }

  [Test]
  public void Subscribe_BuildKSql_ObservesItems()
  {
    //Arrange
    var query = CreateTestableKStreamSet();
      
    var testScheduler = new TestScheduler();

    var results = new List<string>();

    //Act
    using var disposable = query.SubscribeOn(testScheduler).Subscribe(value =>
    {
      results.Add(value);
    }, error => { }, () => { });

    testScheduler.Start();

    //Assert
    Assert.AreEqual(2, results.Count);
  }

  [Test]
  public async Task SubscribeAsync_ObservesItems()
  {
    //Arrange
    var query = CreateTestableKStreamSet();
      
    var testScheduler = new TestScheduler();

    var results = new List<string>();

    //Act
    var subscription = await query.SubscribeOn(testScheduler).SubscribeAsync(value =>
    {
      results.Add(value);
    }, error => { }, () => { });

    testScheduler.Start();

    //Assert
    Assert.AreEqual(2, results.Count);
    subscription.QueryId.Should().Be("xyz");
  }

  [Test]
  public void Source_SchedulersAreNotSet()
  {
    //Arrange
    var source = CreateTestableKStreamSet();

    //Act

    //Assert
    ((KStreamSet)source).SubscribeOnScheduler.Should().BeNull();
    ((KStreamSet)source).ObserveOnScheduler.Should().BeNull();
  }

  [Test]
  public void SubscribeOn_SetsSubscribeOnScheduler()
  {
    //Arrange
    var query = CreateTestableKStreamSet();
      
    var testScheduler = new TestScheduler();

    //Act
    var source = query.SubscribeOn(testScheduler);

    //Assert
    ((KStreamSet)source).SubscribeOnScheduler.Should().Be(testScheduler);
  }

  [Test]
  public void ObserveOn_SetsObserveOnScheduler()
  {
    //Arrange
    var query = CreateTestableKStreamSet();
      
    var testScheduler = new TestScheduler();

    //Act
    var source = query.ObserveOn(testScheduler);

    //Assert
    ((KStreamSet)source).ObserveOnScheduler.Should().Be(testScheduler);
  }

  private IQbservable<Location> CreateStreamSource()
  {
    var context = new TestableDbProvider(TestParameters.KsqlDBUrl);
      
    return context.CreateQueryStream<Location>();
  }

  private IQbservable<string> CreateTestableKStreamSet()
  {
    var context = new TestableDbProvider(TestParameters.KsqlDBUrl);
      
    context.KSqlDbProviderMock.Setup(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
      .Returns(GetTestValues);

    var query = new QueryStream<string> { EnumerableQuery = GetTestValues(), QueryId = "xyz" };

    context.KSqlDbProviderMock.Setup(c => c.RunAsync<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(query);
      
    return context.CreateQueryStream<string>();
  }
    
  [Test]
  public void SelectPredicate_BuildKSql_PrintsPredicate()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => c.Latitude.ToLower() != "HI".ToLower());

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT LCASE({nameof(Location.Latitude)}) != LCASE('HI') FROM Locations EMIT CHANGES;");
  }
    
  [Test]
  public void WhereIsNotNull_BuildKSql_PrintsQuery()
  {
    //Arrange
    var context = new TestableDbProvider(TestParameters.KsqlDBUrl);

    var grouping = context.CreateQueryStream<Click>()
      .Where(c => c.IP_ADDRESS != null)
      .Select(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP });

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    string expectedKSql = @"SELECT IP_ADDRESS, URL, TIMESTAMP FROM Clicks
WHERE IP_ADDRESS IS NOT NULL EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKSql);
  }

  [Test]
  public void WhereIsNull_BuildKSql_PrintsQuery()
  {
    //Arrange
    var context = new TestableDbProvider(TestParameters.KsqlDBUrl);

    var grouping = context.CreateQueryStream<Click>()
      .Where(c => c.IP_ADDRESS == null)
      .Select(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP });

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    string expectedKSql = @"SELECT IP_ADDRESS, URL, TIMESTAMP FROM Clicks
WHERE IP_ADDRESS IS NULL EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKSql);
  }
}
