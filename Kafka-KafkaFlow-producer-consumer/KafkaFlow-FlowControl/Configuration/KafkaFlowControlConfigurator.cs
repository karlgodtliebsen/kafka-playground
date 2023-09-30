using KafkaFlow;
using KafkaFlow.Serializer;

using KafkaFlow_Messages;

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
                            .CreateTopicIfNotExists(config.Topic, 1, 1)
                            .AddProducer(
                                config.ProducerName,
                                producer => producer
                                    .DefaultTopic(config.Topic)
                                    .AddMiddlewares(m => m.AddSingleTypeSerializer<SampleMessage, JsonCoreSerializer>())
                            )
                            .AddConsumer(
                                consumer => consumer
                                    .Topic(config.Topic)
                                    .WithGroupId(config.GroupId)
                                    .WithName(config.ConsumerName)
                                    .WithInitialState(ConsumerInitialState.Stopped)
                                    .WithBufferSize(100)
                                    .WithWorkersCount(1)
                                    .AddMiddlewares(
                                        m => m
                                            .AddSingleTypeSerializer<SampleMessage, JsonCoreSerializer>()
                                            .Add<PrintConsoleMiddleware>()
                                    )
                            );
                    })
        );

        return services;
    }
}