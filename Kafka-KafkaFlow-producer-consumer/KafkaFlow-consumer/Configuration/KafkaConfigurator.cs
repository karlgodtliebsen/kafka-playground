using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.TypedHandler;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consumer.Configuration;

public static class KafkaConfigurator
{
    public static IServiceCollection AddConsumer(this IServiceCollection services, IConfigurationRoot configuration, string? sectionName = null)
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