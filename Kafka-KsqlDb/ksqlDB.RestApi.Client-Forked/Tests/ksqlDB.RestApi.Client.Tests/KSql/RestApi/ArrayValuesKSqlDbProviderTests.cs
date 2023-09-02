using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Ninject;
using NUnit.Framework;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi;

public class ArrayValuesKSqlDbProviderTests : TestBase
{
  [Test]
  public async Task Count_ParseSingleFields_IntegersAreConsumed()
  {
    //Arrange

    //Act
    var results = Run(new { Id = 0, TopK = new double[3] });     

    //Assert
    var resultList = await results.ToListAsync(); 
      
    resultList.Count.Should().Be(3);

    resultList[2].Id.Should().Be(1);
    resultList[2].TopK.Length.Should().Be(2);
    resultList[2].TopK[1].Should().Be(0.00042);
  }
    
  IAsyncEnumerable<T> Run<T>(T anonymousType) {
    var provider = MockingKernel.Get<AggregationsWithArrayResultsKsqlDbQueryStreamProvider>();
    var queryParameters = new QueryStreamParameters();

    var asyncEnumerable = provider.Run<T>(queryParameters);

    return asyncEnumerable;
  }
}
