using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using KafkaFlow.TypedHandler;

using KafkaFlow_Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consumer.Configuration;

public static class KafkaConfigurator
{
    public static IServiceCollection AddConsumer(this IServiceCollection services, IConfigurationRoot configuration, string? sectionName = null)
    {

        sectionName = sectionName ?? KafkaConfiguration.SectionName;
        services.Configure<KafkaConfiguration>(configuration.GetSection(sectionName));
        KafkaConfiguration config = configuration.GetSection(sectionName).Get<KafkaConfiguration>()!;

        services.AddKafka(
            kafka => kafka
                .UseConsoleLog()
                .AddCluster(
                    cluster => cluster
                        .WithBrokers(new[] { config.Broker })
                        .AddConsumer(
                            consumer =>
                            //    consumer.AddConsumer<ProtobufNetSerializer, PrintConsoleMiddleware1>(config, "")
                            consumer.AddConsumer<TestMessage, ProtobufNetSerializer, PrintConsoleHandler1>(config, "")
                            )
                          .AddConsumer(
                            consumer =>
                            // consumer.AddConsumer<ProtobufNetSerializer, PrintConsoleMiddleware2>(config, "xyz")
                            consumer.AddConsumer<TestMessage, ProtobufNetSerializer, PrintConsoleHandler2>(config, "xyz")
                        )
                 )
        );

        return services;
    }

    static IConsumerConfigurationBuilder AddConsumer<TSerializer, TMiddleware>(this IConsumerConfigurationBuilder consumer, KafkaConfiguration config, string groupIdPostfix = "")
        where TSerializer : class, ISerializer
        where TMiddleware : class, IMessageMiddleware
    {
        return consumer
             .Topic(config.Topic)
             //by changing groupid  the two consumers will run simultaneously.
             //Just for this demo, else use it for resillience
             .WithGroupId(config.GroupId + groupIdPostfix)
             .WithBufferSize(100)
             .WithWorkersCount(2)
             .AddMiddlewares(
                 m => m
                     .AddSerializer<TSerializer>()
                     .Add<TMiddleware>())
             ;
    }

    static IConsumerConfigurationBuilder AddConsumer<TMessage, TSerializer, THandler>(this IConsumerConfigurationBuilder consumer, KafkaConfiguration config, string groupIdPostfix = "")
        where TSerializer : class, ISerializer
        where THandler : class, IMessageHandler
    {
        return consumer
                .Topic(config.Topic)
                //by changing groupid  the two consumers will run simultaneously.
                //Just for this demo, else use it for resillience
                .WithGroupId(config.GroupId + groupIdPostfix)
                .WithBufferSize(100)
                .WithWorkersCount(2)
                .AddMiddlewares(
                    m => m
                    .AddSingleTypeSerializer<TSerializer>(typeof(TMessage))
                    .AddTypedHandlers(h => h.AddHandler<THandler>())
                )
            ;
    }

    //static IConsumerMiddlewareConfigurationBuilder AddSingleTypeSerializer<TMessage, TSerializer, THandler>(
    //    this IConsumerMiddlewareConfigurationBuilder middlewares)
    //    where TSerializer : class, ISerializer
    //    where THandler : class, IMessageHandler
    //{
    //    return middlewares
    //        .AddSingleTypeSerializer<TSerializer>(typeof(TMessage))
    //        .AddTypedHandlers(h => h.AddHandler<THandler>());
    //}

    //static IConsumerMiddlewareConfigurationBuilder AddSerializer<TSerializer, T>(
    //   this IConsumerMiddlewareConfigurationBuilder middlewares)
    //   where TSerializer : class, ISerializer
    //   where T : class, IMessageMiddleware
    //{
    //    middlewares.DependencyConfigurator.AddTransient<TSerializer>();
    //    return middlewares
    //        .AddSerializer(resolver => resolver.Resolve<TSerializer>(), _ => new DefaultTypeResolver())
    //        .Add<T>();
    //}

    //class DefaultTypeResolver : IMessageTypeResolver
    //{
    //    private const string MessageType = "Message-Type";

    //    public Type? OnConsume(IMessageContext context)
    //    {
    //        var typeName = context.Headers.GetString(MessageType);
    //        return typeName is null ? null : Type.GetType(typeName);
    //    }

    //    public void OnProduce(IMessageContext context)
    //    {
    //        if (context.Message.Value is null)
    //        {
    //            return;
    //        }
    //        var messageType = context.Message.Value.GetType();
    //        context.Headers.SetString(MessageType, $"{messageType.FullName}, {messageType.Assembly.GetName().Name}");
    //    }
    //}
}