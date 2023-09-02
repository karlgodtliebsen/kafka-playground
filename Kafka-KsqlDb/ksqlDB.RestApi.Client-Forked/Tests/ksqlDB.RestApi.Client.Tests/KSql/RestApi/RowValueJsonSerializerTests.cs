using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnitTests;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi;

public class RowValueJsonSerializerTests : TestBase
{
  private RowValueJsonSerializer ClassUnderTest { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "STRING" },
      ColumnNames = new[] { "NAME" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);
  }

  private record SingleLady
  {
    public string Name { get; set; } = null!;
  }

  [Test]
  public void Deserialize_RecordWithSingleProperty()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "STRING" },
      ColumnNames = new[] { "NAME" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    string rawJson = "[\"f03c278c-61ea-4f69-b153-5647d2eec72e\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<SingleLady>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Name.Should().Be("f03c278c-61ea-4f69-b153-5647d2eec72e");
  }

  [Test]
  public void Deserialize_RecordAsString()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "STRING" },
      ColumnNames = new[] { "KSQL_COL_0" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    string rawJson = "[\"f03c278c-61ea-4f69-b153-5647d2eec72e\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<string>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be("f03c278c-61ea-4f69-b153-5647d2eec72e");
  }

  [Test]
  public void Deserialize_RecordAsDateTimeOffsetStruct()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "STRING" },
      ColumnNames = new[] { "KSQL_COL_0" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    var dateTime = new DateTimeOffset(new DateTime(2022, 9, 23), TimeSpan.FromHours(2));
    var dt = "2022-09-23T00:00:00+02:00";

    string rawJson = $"[\"{dt}\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<DateTimeOffset>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(dateTime);
  }

  [Test]
  public void Deserialize_RecordAsDictionary()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "MAP<STRING, INTEGER>" },
      ColumnNames = new[] { "KSQL_COL_0" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    var value = "{\"a\":1,\"b\":2}";

    string rawJson = $"[{value}]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<Dictionary<string, int>>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value["a"].Should().Be(1);
  }

  [Test]
  public void Deserialize_RecordAsPrimitiveInt()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "INTEGER" },
      ColumnNames = new[] { "KSQL_COL_0" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    var value = 42;

    string rawJson = $"[{value}]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<int>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(value);
  }

  private record MyStruct
  {
    public string Name { get; set; } = null!;
  }

  [Test]
  public void Deserialize_RecordAsRecord()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "STRUCT<`NAME` STRING>" },
      ColumnNames = new[] { "KSQL_COL_0" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    string value = "E.T.";
    string rawJson = "[{\"NAME\":\"E.T.\"}]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<MyStruct>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Name.Should().Be(value);
  }

  public class Movie : Record
  {
    public string Title { get; set; } = null!;
    public int Id { get; set; }
    public int Release_Year { get; set; }
  }

  [Test]
  public void Deserialize_RecordAsClass()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "INTEGER", "STRING", "INTEGER", "BIGINT" },
      ColumnNames = new[] { "ID", "TITLE", "RELEASE_NAME", "ROWTIME" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    string rawJson = "[2,\"Die Hard\",1988,1670438716925]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<Movie>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Id.Should().Be(2);
  }

  private enum MyEnum
  {
    None = 0,
    All = 1
  }

  [Test]
  public void Deserialize_RecordAsEnum()
  {
    //Arrange
    var value = (int)MyEnum.All;

    string rawJson = $"[{value}]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<MyEnum>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(MyEnum.All);
  }

  [Test]
  public void Deserialize_RecordAsGuid()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "STRING" },
      ColumnNames = new[] { "KSQL_COL_0" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    string guid = "f03c278c-61ea-4f69-b153-5647d2eec72e";
    string rawJson = $"[\"{guid}\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<Guid>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(guid);
  }

  class Foo : Dictionary<string, int>
  {
  }

  [Test]
  public void Deserialize_RecordAsDictionaryBase()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "MAP<STRING, INTEGER>" },
      ColumnNames = new[] { "DICT" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);
    string rawJson = "[{\"A\":2}]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<Foo>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value["A"].Should().Be(2);
  }

  [Test]
  public void Deserialize_RecordAsByteArray()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "BYTES" },
      ColumnNames = new[] { "MESSAGE" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    string rawJson = "[\"e30=\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<byte[]>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().BeOfType<byte[]>();
    rowValue.Value[0].Should().Be(0x7b);
    rowValue.Value[1].Should().Be(0x7d);
  }

  [Test]
  public void Deserialize_RecordAsInt()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "INT" },
      ColumnNames = new[] { "MESSAGE" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);

    string rawJson = "[1]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<int>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(1);
  }

  [Test]
  public void DifferentLengthOfColumnNamesAndTypes_ThrowsInvalidOperationException()
  {
    //Arrange
    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new[] { "INT", "STRING" },
      ColumnNames = new[] { "MESSAGE" },
    };

    //Assert
    Assert.ThrowsException<InvalidOperationException>(() =>
    {
      //Act
      ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);
    });
  }
}
