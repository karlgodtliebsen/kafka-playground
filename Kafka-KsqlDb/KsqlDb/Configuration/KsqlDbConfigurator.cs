using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using KsqlDb.Domain;

namespace KsqlDb.Configuration;

public static class KsqlDbConfigurator
{

    public static IServiceCollection AddKsqlDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KsqlDbConfiguration>(configuration.GetSection(KsqlDbConfiguration.SectionName));
        services.AddTransient<KsqlDbAdminClient>();

        //services.AddTransient<KafkaConsumer>();
        //services.AddTransient<KafkaProducer>();
        //services.AddTransient<KafkaStreaming>();
        return services;
    }
}

