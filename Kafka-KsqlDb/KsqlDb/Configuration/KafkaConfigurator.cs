using KsqlDb.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KsqlDb.Configuration;

public static class KafkaConfigurator
{

    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaConfiguration>(configuration.GetSection(KafkaConfiguration.SectionName));
        //services.AddTransient<KafkaConsumer>();
        //services.AddTransient<KafkaProducer>();
        services.AddTransient<KafkaAdminClient>();
        //services.AddTransient<KafkaStreaming>();
        return services;
    }
    
}