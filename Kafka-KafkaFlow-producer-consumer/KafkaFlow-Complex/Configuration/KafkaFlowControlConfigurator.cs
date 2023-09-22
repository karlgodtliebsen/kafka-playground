using KafkaFlow;
using KafkaFlow.Serializer;

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
                            .CreateTopicIfNotExists(config.OutboundMapTopic.ToLowerInvariant(), 1, 1)

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
                            .AddProducer(
                                config.DataSourceProducerName,
                                producer => producer
                                    .DefaultTopic(config.OutboundMapTopic.ToLowerInvariant())
                                    .AddMiddlewares(m => m.AddSingleTypeSerializer<OutboundMessage, JsonCoreSerializer>())
                            )
                            ;
                    })
        );

        return services;
    }
}