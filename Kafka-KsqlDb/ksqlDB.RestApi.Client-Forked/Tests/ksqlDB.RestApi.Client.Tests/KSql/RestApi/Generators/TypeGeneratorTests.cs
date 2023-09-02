using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Generators;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Generators;

public class TypeGeneratorTests
{
  [Test]
  public void CreateType()
  {      
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<Address>();

    //Assert
    statement.Should().Be($@"CREATE TYPE {nameof(Address).ToUpper()} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_WithTypeName()
  {      
    //Arrange
    string typeName = "MyType";

    //Act
    string statement = new TypeGenerator().Print<Address>(typeName);

    //Assert
    statement.Should().Be($@"CREATE TYPE {typeName} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_NestedType()
  {      
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<Person>();

    //Assert
    statement.Should().Be($@"CREATE TYPE {nameof(Person).ToUpper()} AS STRUCT<Name VARCHAR, Address ADDRESS>;");
  }

  [Test]
  public void CreateType_BytesType()
  {      
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<Thumbnail>();

    //Assert
    statement.Should().Be(@$"CREATE TYPE {nameof(Thumbnail).ToUpper()} AS STRUCT<Image BYTES>;");
  }

  [Test]
  public void CreateType_MapType()
  {      
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<Container>();

    //Assert
    statement.Should().Be(@$"CREATE TYPE {nameof(Container).ToUpper()} AS STRUCT<Values2 MAP<VARCHAR, INT>>;");
  }

  public record Address
  {
    public int Number { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
  }

  public class Person
  {
    public string Name { get; set; } = null!;
    public Address Address { get; set; } = null!;
  }

  public struct Thumbnail
  {
    public byte[] Image { get; set; }
  }

  record Container
  {
    public IDictionary<string, int> Values2 { get; set; } = null!;
  }

  #region GenericType

  record DatabaseChangeObject<TEntity> : DatabaseChangeObject
  {
    public TEntity Before { get; set; } = default!;
    public TEntity After { get; set; } = default!;
  }

  record DatabaseChangeObject
  {
    public Source Source { get; set; } = null!;
    public string Op { get; set; } = null!;

    public long TsMs { get; set; }

    public ChangeDataCaptureType OperationType => ChangeDataCaptureType.Created;
  }

  [Flags]
  enum ChangeDataCaptureType
  {
    Read,
    Created,
    Updated,
    Deleted
  }

  record IoTSensor
  {
    [Key]
    public string SensorId { get; set; } = null!;
    public int Value { get; set; }
  }

  public record Source
  {
    public string Version { get; set; } = null!;
    public string Connector { get; set; } = null!;
  }

  [Test]
  public void CreateType_GenericType()
  {      
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<DatabaseChangeObject<IoTSensor>>();

    //Assert
    statement.Should().Be(@"CREATE TYPE DATABASECHANGEOBJECT AS STRUCT<Before IOTSENSOR, After IOTSENSOR, Source SOURCE, Op VARCHAR, TsMs BIGINT>;"); //, Transaction OBJECT
  }

  #endregion
}
