using KsqlDb.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KsqlDb.Configuration;

public static class KafkaConfigurator
{

    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaConfiguration>(configuration.GetSection(KafkaConfiguration.SectionName));
        services.AddTransient<KafkaAdminClient>();
        return services;
    }
    
}