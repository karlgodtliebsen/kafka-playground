﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Connector.Cdc.Connectors;
using UnitTests;

namespace SqlServer.Connector.Tests.Cdc.Connectors;

[TestClass]
public class SqlServerConnectorMetadataTests : TestBase<SqlServerConnectorMetadata>
{
  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    string connectionString =
      "Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

    ClassUnderTest = new SqlServerConnectorMetadata(connectionString);
  }

  [TestMethod]
  public void ConnectorClass()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.ConnectorClass.Should().Be("io.debezium.connector.sqlserver.SqlServerConnector");
  }

  [TestMethod]
  public void DatabaseHostnameName()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabaseHostname.Should().Be("127.0.0.1");
  }

  [TestMethod]
  public void DatabaseUser()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabaseUser.Should().Be("SA");
  }

  [TestMethod]
  public void DefaultCtor_Port()
  {
    //Arrange
    ClassUnderTest = new SqlServerConnectorMetadata();

    //Act

    //Assert
    ClassUnderTest.DatabasePort.Should().Be("1433");
  }

  [TestMethod]
  public void Port()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabasePort.Should().Be("1433");
  }

  [TestMethod]
  public void DatabasePassword()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabasePassword.Should().Be("<YourNewStrong@Passw0rd>");
  }

  [TestMethod]
  public void DatabaseDbname()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.DatabaseDbname.Should().Be("Sensors");
  }

  [TestMethod]
  public void TrySetDatabaseHistoryKafkaTopic()
  {
    //Arrange
    ClassUnderTest.DatabaseServerName = "GAIA";

    //Act
    ClassUnderTest.TrySetDatabaseHistoryKafkaTopic();

    //Assert
    ClassUnderTest.DatabaseHistoryKafkaTopic.Should().Be($"dbhistory.{ClassUnderTest.DatabaseServerName}");
  }

  [TestMethod]
  public void TrySetConnectorName()
  {
    //Arrange

    //Act
    ClassUnderTest.TrySetConnectorName();

    //Assert
    ClassUnderTest.Name.Should().Be($"{ClassUnderTest.DatabaseDbname}-connector");
  }

  [TestMethod]
  public void KafkaBootstrapServers()
  {
    //Arrange
    string bootstrapServers = "localhost:9092,broker01:29092";

    //Act
    ClassUnderTest.KafkaBootstrapServers = bootstrapServers;

    //Assert
    ClassUnderTest.KafkaBootstrapServers.Should().Be(bootstrapServers);
  }
}