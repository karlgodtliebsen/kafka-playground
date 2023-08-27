using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WikiEditStream.Configuration;

public static class KafkaConfigurator
{


    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaConfiguration>(configuration.GetSection(KafkaConfiguration.SectionName));
        services.AddTransient<ConfluentKafkaDockerComposeBuilder>();
        services.AddTransient<KafkaConsumer>();
        services.AddTransient<KafkaProducer>();
        services.AddTransient<KafkaAdminClient>();

        return services;
    }

}