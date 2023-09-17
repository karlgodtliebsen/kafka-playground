using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Options;

using System.Text;

using WikiEditStream.Configuration;

namespace WikiEditStream.DockerSupport;

public sealed class ConfluentKafkaDockerComposeBuilder : IAsyncDisposable
{
    private IContainer? brokerContainer;
    private IContainer? schemaRegistryContainer;
    private IContainer? connectContainer;
    private IContainer? ksqlDbServerContainer;
    private IContainer? controlCenterContainer;
    private IContainer? ksqlDataGenContainer;
    private IContainer? restProxyContainer;
    private IContainer? ksqlClientContainer;
    private bool isDisposed = false;
    private readonly KafkaConfiguration config;

    public ConfluentKafkaDockerComposeBuilder(IOptions<KafkaConfiguration> options)
    {
        config = options.Value;
    }

    public async Task StopConfluentDocker()
    {
        if (isDisposed) return;
        isDisposed = true;
        if (brokerContainer is not null)
        {
            await brokerContainer!.StopAsync().ConfigureAwait(false);
        }
        if (schemaRegistryContainer is not null)
        {
            await schemaRegistryContainer!.StopAsync().ConfigureAwait(false);
        }
        if (connectContainer is not null)
        {
            await connectContainer!.StopAsync().ConfigureAwait(false);
        }
        if (ksqlDbServerContainer is not null)
        {
            await ksqlDbServerContainer!.StopAsync().ConfigureAwait(false);
        }
        if (controlCenterContainer is not null)
        {
            await controlCenterContainer!.StopAsync().ConfigureAwait(false);
        }
        if (ksqlClientContainer is not null)
        {
            await ksqlClientContainer!.StopAsync().ConfigureAwait(false);
        }
        if (ksqlDataGenContainer is not null)
        {
            await ksqlDataGenContainer!.StopAsync().ConfigureAwait(false);
        }
        if (restProxyContainer is not null)
        {
            await restProxyContainer!.StopAsync().ConfigureAwait(false);
        }

    }
    public async ValueTask DisposeAsync()
    {
        if (isDisposed) return;
        await StopConfluentDocker();
        isDisposed = true;
    }

    public string ClusterId { get; private set; } = "AkU3OEVBNTcwNTJENDM2Qg";//Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    public int KafkaRestPort { get; private set; } = 8082;
    public int BrokerPlainTextPort { get; private set; } = 9092;
    public int BrokerJmxPort { get; private set; } = 9101;
    public int SchemaRegistryPort { get; private set; } = 8081;
    public int ConnectPort { get; private set; } = 8083;
    public int KsqlDbPort { get; private set; } = 8088;
    public int ControlCenterPort { get; private set; } = 9021;

    public string PortData()
    {
        var sb = new StringBuilder();
        sb.Append("ClusterId -> ").AppendLine(ClusterId);
        sb.AppendLine($"ControlCenterPort -> http://localhost:{ControlCenterPort}/clusters");
        sb.AppendLine($"ControlCenterPort -> http://localhost:{ControlCenterPort}/clusters/{ClusterId}/overview");
        sb.AppendLine($"KafkaRestPort -> http://localhost:{KafkaRestPort}");
        sb.AppendLine($"BrokerPlainTextPort -> http://localhost:{BrokerPlainTextPort}");
        sb.AppendLine($"BrokerJmxPort -> http://localhost:{BrokerJmxPort}");
        sb.AppendLine($"SchemaRegistryPort -> http://localhost:{SchemaRegistryPort}");
        sb.AppendLine($"ConnectPort -> http://localhost:{ConnectPort}");
        sb.AppendLine($"KsqlDbPort -> http://localhost:{KsqlDbPort}");
        return sb.ToString();
    }

    private void AdaptConfiguration()
    {
        config.BootstrapServers = $"localhost:{BrokerPlainTextPort}";
    }

    ////The builder also starts the containers in order to get port mappings
    public async Task BuildAndStartConfluentDocker()
    {
        var brokerContainerName = "broker-testcontainer";
        var schemaRegistryContainerName = "schemaRegistry-testcontainer";
        var connectContainerName = "connect-testcontainer";
        var ksqlDbServerContainerName = "ksqldb-server-testcontainer";
        var controlCenterContainerName = "control-center-testcontainer";
        var ksqlClientContainerName = "ksqldb-cli-testcontainer";
        var ksqlDataGenContainerName = "ksql-datagen-testcontainer";
        var restProxyContainerName = "rest-proxy-testcontainer";

        const int brokerPort = 29092;
        const int brokerQuorumPort = 29093;

        var network = new NetworkBuilder()
            .WithName("app-network")
            .WithDriver(NetworkDriver.Bridge)
            .Build();

        brokerContainer = new ContainerBuilder()
            .WithHostname(brokerContainerName)
            .WithName(brokerContainerName)
            .WithImage("confluentinc/cp-kafka:7.4.1")
            .WithPortBinding(BrokerPlainTextPort, BrokerPlainTextPort)      //Default 9092
            .WithPortBinding(BrokerJmxPort, BrokerJmxPort)                  //Default 9101
            .WithNetwork(network)
            .WithEnvironment("KAFKA_NODE_ID", "1")
            .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT")
            .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", $"PLAINTEXT://{brokerContainerName}:{brokerPort},PLAINTEXT_HOST://localhost:{BrokerPlainTextPort}")
            .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
            .WithEnvironment("KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS", "0")
            .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
            .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1")
            .WithEnvironment("KAFKA_JMX_PORT", $"{BrokerJmxPort}")
            .WithEnvironment("KAFKA_JMX_HOSTNAME", "localhost")
            .WithEnvironment("KAFKA_PROCESS_ROLES", "broker,controller")
            .WithEnvironment("KAFKA_CONTROLLER_QUORUM_VOTERS", $"1@{brokerContainerName}:{brokerQuorumPort}")
            .WithEnvironment("KAFKA_LISTENERS", $"PLAINTEXT://{brokerContainerName}:{brokerPort},CONTROLLER://{brokerContainerName}:{brokerQuorumPort},PLAINTEXT_HOST://0.0.0.0:{BrokerPlainTextPort}")
            .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "PLAINTEXT")
            .WithEnvironment("KAFKA_CONTROLLER_LISTENER_NAMES", "CONTROLLER")
            .WithEnvironment("KAFKA_LOG_DIRS", "/tmp/kraft-combined-logs")
             //.WithEnvironment("CLUSTER_ID", "MkU3OEVBNTcwNTJENDM2Qk")
             .WithEnvironment("CLUSTER_ID", ClusterId)
            .Build();
        await brokerContainer!.StartAsync().ConfigureAwait(false);
        BrokerPlainTextPort = brokerContainer.GetMappedPublicPort(BrokerPlainTextPort);
        BrokerJmxPort = brokerContainer.GetMappedPublicPort(BrokerJmxPort);



        schemaRegistryContainer = new ContainerBuilder()
            .WithHostname(schemaRegistryContainerName)
            .WithName(schemaRegistryContainerName)
            .WithImage("confluentinc/cp-schema-registry:7.4.1")
            .WithNetwork(network)
            .WithPortBinding(SchemaRegistryPort, SchemaRegistryPort)
            .DependsOn(brokerContainer)
            .WithEnvironment("SCHEMA_REGISTRY_HOST_NAME", schemaRegistryContainerName)
            .WithEnvironment("SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS", $"{brokerContainerName}:{brokerPort}")
            .WithEnvironment("SCHEMA_REGISTRY_LISTENERS", $"http://0.0.0.0:{SchemaRegistryPort}")
            .Build();

        await schemaRegistryContainer!.StartAsync().ConfigureAwait(false);
        SchemaRegistryPort = schemaRegistryContainer.GetMappedPublicPort(SchemaRegistryPort);

        connectContainer = new ContainerBuilder()
            .WithHostname(connectContainerName)
            .WithName(connectContainerName)
            .WithImage("cnfldemos/cp-server-connect-datagen:0.5.3-7.1.0")
            .WithNetwork(network)
            .WithPortBinding(ConnectPort, ConnectPort)
            .DependsOn(brokerContainer)
            .DependsOn(schemaRegistryContainer)
            .WithEnvironment("CONNECT_BOOTSTRAP_SERVERS", $"{brokerContainerName}:{brokerPort}")
            .WithEnvironment("CONNECT_REST_ADVERTISED_HOST_NAME", connectContainerName)
            .WithEnvironment("CONNECT_GROUP_ID", "compose-connect-group")
            .WithEnvironment("CONNECT_CONFIG_STORAGE_TOPIC", "docker-connect-configs")
            .WithEnvironment("CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR", "1")
            .WithEnvironment("CONNECT_OFFSET_FLUSH_INTERVAL_MS", "10000")
            .WithEnvironment("CONNECT_OFFSET_STORAGE_TOPIC", "docker-connect-ffsets")
            .WithEnvironment("CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR", "1")
            .WithEnvironment("CONNECT_STATUS_STORAGE_TOPIC", "docker-connect-status")
            .WithEnvironment("CONNECT_STATUS_STORAGE_REPLICATION_FACTOR", "1")
            .WithEnvironment("CONNECT_KEY_CONVERTER", "org.apache.kafka.connect.storage.StringConverter")
            .WithEnvironment("CONNECT_VALUE_CONVERTER", "io.confluent.connect.avro.AvroConverter")
            .WithEnvironment("CONNECT_VALUE_CONVERTER_SCHEMA_REGISTRY_URL", $"http://{schemaRegistryContainerName}:{SchemaRegistryPort}")
            .WithEnvironment("CLASSPATH", "/usr/share/java/monitoring-interceptors/monitoring-interceptors-7.4.1.jar")
            .WithEnvironment("CONNECT_PRODUCER_INTERCEPTOR_CLASSES", "io.confluent.monitoring.clients.interceptor.MonitoringProducerInterceptor")
            .WithEnvironment("CONNECT_CONSUMER_INTERCEPTOR_CLASSES", "io.confluent.monitoring.clients.interceptor.MonitoringConsumerInterceptor")
            .WithEnvironment("CONNECT_PLUGIN_PATH", "/usr/share/java,/usr/share/confluent-hub-components")
            .WithEnvironment("CONNECT_LOG4J_LOGGERS", "org.apache.zookeeper=ERROR,org.I0Itec.zkclient=ERROR,org.reflections=ERROR")
            .Build();

        await connectContainer!.StartAsync().ConfigureAwait(false);
        ConnectPort = connectContainer.GetMappedPublicPort(ConnectPort);

        ksqlDbServerContainer = new ContainerBuilder()
            .WithHostname(ksqlDbServerContainerName)
            .WithName(ksqlDbServerContainerName)
            .WithImage("confluentinc/cp-ksqldb-server:7.4.1")
            .WithNetwork(network)
            .WithPortBinding(KsqlDbPort, KsqlDbPort)
            .DependsOn(brokerContainer)
            .DependsOn(connectContainer)
            .WithEnvironment("KSQL_CONFIG_DIR", "/etc/ksql")
            .WithEnvironment("KSQL_BOOTSTRAP_SERVERS", $"{brokerContainerName}:{brokerPort}")
            .WithEnvironment("KSQL_HOST_NAME", $"{ksqlDbServerContainerName}")
            .WithEnvironment("KSQL_LISTENERS", $"http://0.0.0.0:{KsqlDbPort}")
            .WithEnvironment("KSQL_CACHE_MAX_BYTES_BUFFERING", "0")
            .WithEnvironment("KSQL_KSQL_SCHEMA_REGISTRY_URL", $"http://{schemaRegistryContainerName}:{SchemaRegistryPort}")
            .WithEnvironment("KSQL_PRODUCER_INTERCEPTOR_CLASSES", "io.confluent.monitoring.clients.interceptor.MonitoringProducerInterceptor")
            .WithEnvironment("KSQL_CONSUMER_INTERCEPTOR_CLASSES", "io.confluent.monitoring.clients.interceptor.MonitoringConsumerInterceptor")
            .WithEnvironment("KSQL_KSQL_CONNECT_URL", $"http://{connectContainerName}:{ConnectPort}")
            .WithEnvironment("KSQL_KSQL_LOGGING_PROCESSING_TOPIC_REPLICATION_FACTOR", "1")
            .WithEnvironment("KSQL_KSQL_LOGGING_PROCESSING_TOPIC_AUTO_CREATE", "true")
            .WithEnvironment("KSQL_KSQL_LOGGING_PROCESSING_STREAM_AUTO_CREATE", "true")
            .Build();

        await ksqlDbServerContainer!.StartAsync().ConfigureAwait(false);
        KsqlDbPort = ksqlDbServerContainer.GetMappedPublicPort(KsqlDbPort);

        controlCenterContainer = new ContainerBuilder()
            .WithImage("confluentinc/cp-enterprise-control-center:7.4.1")
            .WithHostname(controlCenterContainerName)
            .WithName(controlCenterContainerName)
            .WithNetwork(network)
            .WithPortBinding(ControlCenterPort, ControlCenterPort)
            .DependsOn(brokerContainer)
            .DependsOn(schemaRegistryContainer)
            .DependsOn(connectContainer)
            .DependsOn(ksqlDbServerContainer)
            .WithEnvironment("CONTROL_CENTER_BOOTSTRAP_SERVERS", $"{brokerContainerName}:{brokerPort}")
            .WithEnvironment("CONTROL_CENTER_CONNECT_CONNECT-DEFAULT_CLUSTER", $"{connectContainerName}:{ConnectPort}")
            .WithEnvironment("CONTROL_CENTER_KSQL_KSQLDB1_URL", $"http://{ksqlDbServerContainerName}:{KsqlDbPort}")
            .WithEnvironment("CONTROL_CENTER_KSQL_KSQLDB1_ADVERTISED_URL", $"http://localhost:{KsqlDbPort}")
            .WithEnvironment("CONTROL_CENTER_SCHEMA_REGISTRY_URL", $"http://{schemaRegistryContainerName}:{SchemaRegistryPort}")
            .WithEnvironment("CONTROL_CENTER_REPLICATION_FACTOR", "1")
            .WithEnvironment("CONTROL_CENTER_INTERNAL_TOPICS_PARTITIONS", "1")
            .WithEnvironment("CONTROL_CENTER_MONITORING_INTERCEPTOR_TOPIC_PARTITIONS", "1")
            .WithEnvironment("CONFLUENT_METRICS_TOPIC_REPLICATION", "1")
            .WithEnvironment("PORT", $"{ControlCenterPort}")
            .Build();
        await controlCenterContainer!.StartAsync().ConfigureAwait(false);
        ControlCenterPort = controlCenterContainer.GetMappedPublicPort(ControlCenterPort);

        ksqlClientContainer = new ContainerBuilder()
            .WithImage("confluentinc/cp-ksqldb-cli:7.4.1")
            .WithHostname(ksqlClientContainerName)
            .WithName(ksqlClientContainerName)
            .WithNetwork(network)
            .DependsOn(brokerContainer)
            .DependsOn(connectContainer)
            .DependsOn(ksqlDbServerContainer)
            .WithEntrypoint("/bin/sh")
            .WithCreateParameterModifier((r) => r.Tty = true)
            .Build();

        await ksqlClientContainer!.StartAsync().ConfigureAwait(false);

        //string bashCommand = $"""
        //            bash -c 'echo Waiting for Kafka to be ready... && \
        //            cub kafka-ready -b {brokerName}:{brokerPort} 1 40 && \
        //            echo Waiting for Confluent Schema Registry to be ready... && \
        //            cub sr-ready {schemaRegistryName} {SchemaRegistryPort} 40 && \
        //            echo Waiting a few seconds for topic creation to finish... && \
        //            sleep 11'
        //        """;

        string bashCommand = $"""bash -c 'echo Waiting for Kafka to be ready... && sleep 11'""";

        ksqlDataGenContainer = new ContainerBuilder()
            .WithHostname(ksqlDataGenContainerName)
            .WithName(ksqlDataGenContainerName)
            .WithImage("confluentinc/ksqldb-examples:7.4.1")
            .WithNetwork(network)
            .DependsOn(ksqlDbServerContainer)
            .DependsOn(brokerContainer)
            .DependsOn(schemaRegistryContainer)
            .DependsOn(connectContainer)
            //.WithCommand(bashCommand)
            .WithEnvironment("KSQL_CONFIG_DIR", "/etc/ksql")
            .WithEnvironment("STREAMS_BOOTSTRAP_SERVERS", $"{brokerContainerName}:{brokerPort}")
            .WithEnvironment("STREAMS_SCHEMA_REGISTRY_HOST", $"{schemaRegistryContainerName}")
            .WithEnvironment("STREAMS_SCHEMA_REGISTRY_PORT", $"{SchemaRegistryPort}")
            .Build();

        await Task.Delay(10000);
        await ksqlDataGenContainer!.StartAsync().ConfigureAwait(false);

        restProxyContainer = new ContainerBuilder()
            .WithImage("confluentinc/cp-kafka-rest:7.4.1")
            .WithHostname(restProxyContainerName)
            .WithName(restProxyContainerName)
            .WithPortBinding(KafkaRestPort, KafkaRestPort)
            .WithNetwork(network)
            .DependsOn(brokerContainer)
            .DependsOn(schemaRegistryContainer)
            .WithEnvironment("KAFKA_REST_HOST_NAME", restProxyContainerName)
            .WithEnvironment("KAFKA_REST_BOOTSTRAP_SERVERS", $"{brokerContainerName}:{brokerPort}")
            .WithEnvironment("KAFKA_REST_LISTENERS", $"http://0.0.0.0:{KafkaRestPort}")
            .WithEnvironment("KAFKA_REST_SCHEMA_REGISTRY_URL", $"http://{schemaRegistryContainerName}:{SchemaRegistryPort}")
            .Build();

        await restProxyContainer!.StartAsync().ConfigureAwait(false);
        KafkaRestPort = restProxyContainer.GetMappedPublicPort(KafkaRestPort);
        AdaptConfiguration();
    }

}

