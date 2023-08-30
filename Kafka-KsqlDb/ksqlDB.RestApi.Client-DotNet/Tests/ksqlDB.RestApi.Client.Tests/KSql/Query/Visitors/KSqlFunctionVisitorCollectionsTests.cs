using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

public class KSqlFunctionVisitorCollectionsTests : TestBase
{
  private KSqlFunctionVisitor ClassUnderTest { get; set; } = null!;

  private StringBuilder StringBuilder { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlFunctionVisitor(StringBuilder, new KSqlQueryMetadata());
  }

  #region Collection functions

  private class Collection
  {
    public int[] Items1 { get; set; } = null!;
    public int[] Items2 { get; set; } = null!;
  }

  #region ArrayContains

  [Test]
  public void ArrayContains_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, bool>> expression = c => K.Functions.ArrayContains(c.Items1, 2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_CONTAINS({nameof(Collection.Items1)}, 2)");
  }

  [Test]
  public void Array_BuildKSql_PrintsArrayFromProperties()
  {
    //Arrange
    Expression<Func<Tweet, int[]>> expression = c => new[] { c.Id, c.Id };

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY[{nameof(Tweet.Id)}, {nameof(Tweet.Id)}]");
  }

  #endregion

  #region ArrayDistinct

  [Test]
  public void ArrayDistinct_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayDistinct(c.Items1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_DISTINCT({nameof(Collection.Items1)})");
  }


  #endregion

  #region ArrayExcept

  [Test]
  public void ArrayExcept_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayExcept(c.Items1, c.Items2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_EXCEPT({nameof(Collection.Items1)}, {nameof(Collection.Items2)})");
  }

  #endregion

  #region ArrayIntersect

  [Test]
  public void ArrayIntersect_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayIntersect(c.Items1, c.Items2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_INTERSECT({nameof(Collection.Items1)}, {nameof(Collection.Items2)})");
  }

  #endregion

  #region ArrayJoin

  [Test]
  public void ArrayJoin_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, string>> expression = c => K.Functions.ArrayJoin(c.Items1, ";");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_JOIN({nameof(Collection.Items1)}, ';')");
  }

  #endregion

  #region ArrayRemove

  [Test]
  public void ArrayRemove_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayRemove(c.Items1, 1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_REMOVE({nameof(Collection.Items1)}, 1)");
  }

  #endregion

  #region ArrayLength

  [Test]
  public void ArrayLength_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int?>> expression = c => K.Functions.ArrayLength(c.Items1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_LENGTH({nameof(Collection.Items1)})");
  }

  #endregion

  #region ArrayMin

  [Test]
  public void ArrayMin_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int?>> expression = c => K.Functions.ArrayMin(c.Items1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_MIN({nameof(Collection.Items1)})");
  }

  #endregion

  #region ArrayMax

  [Test]
  public void ArrayMax_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int?>> expression = c => K.Functions.ArrayMax(c.Items1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_Max({nameof(Collection.Items1)})");
  }

  #endregion

  #region ArraySort

  [Test]
  public void ArraySortNewArray_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int?[]>> expression = c => K.Functions.ArraySort(new int?[]{ 3, null, 1}, ListSortDirection.Ascending);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("ARRAY_SORT(ARRAY[3, NULL, 1], 'ASC')");
  }

  [Test]
  public void ArraySort_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int[]>> expression = c => K.Functions.ArraySort(c.Items1, ListSortDirection.Descending);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_SORT({nameof(Collection.Items1)}, 'DESC')");
  }

  #endregion

  #region ArrayUnion

  [Test]
  public void ArrayUnionNewArray_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int?[]>> expression = c => K.Functions.ArrayUnion(new int?[]{ 3, null, 1}, new int?[]{ 3, null});

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("ARRAY_UNION(ARRAY[3, NULL, 1], ARRAY[3, NULL])");
  }

  [Test]
  public void ArrayUnion_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayUnion(c.Items1, c.Items1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ARRAY_UNION({nameof(Collection.Items1)}, {nameof(Collection.Items1)})");
  }

  #endregion

  #region AsMap

  [Test]
  public void AsMap_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, IDictionary<string, int>>> expression = _ => K.Functions.AsMap(new []{ "1", "2" }, new []{ 11, 22 });

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("AS_MAP(ARRAY['1', '2'], ARRAY[11, 22])");
  }

  #endregion

  #region JsonArrayContains

  [Test]
  public void JsonArrayContains_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, bool>> expression = _ => K.Functions.JsonArrayContains("[1, 2, 3]", 2);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("JSON_ARRAY_CONTAINS('[1, 2, 3]', 2)");
  }

  #endregion

  #region MapKeys

  [Test]
  public void MapKeys_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Collection, string[]>> expression = c => K.Functions.MapKeys(new Dictionary<string, int>
    {
      {"apple", 10},
      {"banana", 20}
    });

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("MAP_KEYS(MAP('apple' := 10, 'banana' := 20))");
  }

  #endregion

  #endregion
}
