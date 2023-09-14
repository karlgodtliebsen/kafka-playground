using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.TypedHandler;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Producer.Configuration;

public static class KafkaConfigurator
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfigurationRoot configuration, string? sectionName = null)
    {

        sectionName = sectionName ?? KafkaConfiguration.SectionName;
        services.Configure<KafkaConfiguration>(configuration.GetSection(sectionName));
        var config = configuration.GetSection(sectionName).Get<KafkaConfiguration>()!;

        services.AddKafka(
            kafka => kafka
                .UseConsoleLog()
                .AddCluster(
                    cluster => cluster
                        .WithBrokers(new[] { config.Broker })
                        .CreateTopicIfNotExists(config.Topic, 6, 1)//config
                        .AddProducer(config.ProducerName,
                            producer => producer
                                .DefaultTopic(config.Topic)
                                .AddMiddlewares(m => m.AddSerializer<ProtobufNetSerializer>())
                        )
                        .AddConsumer(
                            consumer => consumer
                                .Topic(config.Topic)
                                .WithGroupId(config.GroupId)
                                .WithBufferSize(100)
                                .WithWorkersCount(3)
                                .AddMiddlewares(
                                    m => m
                                        .AddSerializer<ProtobufNetSerializer>()
                                        .AddTypedHandlers(h => h.AddHandler<PrintConsoleHandler>())
                                )
                        )
                )
        );

        return services;
    }
}
public class KafkaConfiguration
{
    public const string SectionName = "Kafka";
    public string? Broker { get; set; } = default!;
    public string Topic { get; set; } = default!;
    public string GroupId { get; set; } = default!;
    public string ProducerName { get; set; } = default!;

}