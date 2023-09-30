using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;

using KafkaFlow_Messages;

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
                        .CreateTopicIfNotExists(config.Topic, 1, 1)
                        .AddProducer(config.ProducerName,
                            //producer => producer.AddProducer<ProtobufNetSerializer>(config)
                            producer => producer.AddProducer<TestMessage, ProtobufNetSerializer>(config)
                        )
                )
        );

        return services;
    }

    //m.AddSerializer<KafkaFlow.Serializer.JsonCoreSerializer>())
    //m.AddSerializer<KafkaFlow.Serializer.ProtoBuf.ProtobufMessageSerializer>())

    //m.AddSchemaRegistryAvroSerializer<KafkaFlow.Serializer.SchemaRegistry.ConfluentAvroSerializer>))
    //m.AddSchemaRegistryJsonSerializer<KafkaFlow.Serializer.SchemaRegistry.ConfluentJsonSerializer>))
    //m.AddSchemaRegistryProtobufSerializer<KafkaFlow.Serializer.SchemaRegistry.ConfluentProtobufSerializer>))

    //m.AddSerializer<KafkaFlow.Serializer.MessagePackSerializer>))
    //m.AddSerializer<KafkaFlow.Serializer.ApacheAvro.ApacheAvroMessageSerializer>))
    //m.AddSerializer<KafkaFlow.Serializer.Json.JsonMessageSerializer>))


    static IProducerConfigurationBuilder AddProducer<TSerializer>(
       this IProducerConfigurationBuilder producer, KafkaConfiguration config)
       where TSerializer : class, ISerializer
    {
        return producer
                .DefaultTopic(config.Topic)
                .AddMiddlewares(m => m.AddSerializer<TSerializer>())
            ;
    }

    static IProducerConfigurationBuilder AddProducer<TestMessage, TSerializer>(
        this IProducerConfigurationBuilder producer, KafkaConfiguration config)
        where TSerializer : class, ISerializer
    {
        return producer
                .DefaultTopic(config.Topic)
                .AddMiddlewares(m => m.AddSingleTypeSerializer<TestMessage, TSerializer>())
            ;
    }
}