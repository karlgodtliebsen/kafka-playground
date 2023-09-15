using KafkaFlow;
using KafkaFlow.Serializer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Producer.Configuration;

public static class KafkaConfigurator
{
    public static IServiceCollection AddProducer(this IServiceCollection services, IConfigurationRoot configuration, string? sectionName = null)
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
                )
        );

        return services;
    }
}