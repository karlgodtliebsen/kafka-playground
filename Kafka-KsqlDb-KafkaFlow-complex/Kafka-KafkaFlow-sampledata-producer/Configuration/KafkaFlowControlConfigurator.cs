using KafkaFlow;
using KafkaFlow.Serializer;

using KsqlDb.Domain;
using KsqlDb.HostServices;

using Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowControl.Configuration;

public static class KafkaFlowControlConfigurator
{
    public static IServiceCollection AddFlowControl(this IServiceCollection services, IConfigurationRoot configuration, string? sectionName = null)
    {

        sectionName = sectionName ?? KafkaFlowConfiguration.SectionName;
        services.Configure<KafkaFlowConfiguration>(configuration.GetSection(sectionName));
        var config = configuration.GetSection(sectionName).Get<KafkaFlowConfiguration>()!;
        services.AddTransient<DataSourceProducer>();
        services.AddHostedService<ProducerService>();

        services.AddKafka(
            kafka => kafka
                .UseConsoleLog()
                .AddCluster(
                    cluster =>
                    {
                        cluster
                            .WithBrokers(new[] { config.Broker })
                            .CreateTopicIfNotExists(config.DataSourceTopic.ToLowerInvariant(), 1, 1)
                            .CreateTopicIfNotExists(config.IdentityMapTopic.ToLowerInvariant(), 1, 1)
                            .AddProducer(
                                config.IdentityMapProducerName,
                                producer => producer
                                    .DefaultTopic(config.IdentityMapTopic.ToLowerInvariant())
                                    .AddMiddlewares(m => m.AddSingleTypeSerializer<IdentityMapMessage, JsonCoreSerializer>())
                            )
                            .AddProducer(
                                config.DataSourceProducerName,
                                producer => producer
                                    .DefaultTopic(config.DataSourceTopic.ToLowerInvariant())
                                    .AddMiddlewares(m => m.AddSingleTypeSerializer<DataSourceMessage, JsonCoreSerializer>())
                            )
                            ;
                    })
        );

        return services;
    }
}