using KafkaFlow;

using ksqlDb.RestApi.Client.DependencyInjection;

using KsqlDb.DbContext;
using KsqlDb.Domain;
using KsqlDb.HostServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KsqlDb.Configuration;

public static class KsqlDbConfigurator
{

    public static IServiceCollection AddKsqlDb(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection(KsqlDbConfiguration.SectionName).Get<KsqlDbConfiguration>()!;
        services.AddSingleton(Options.Create(config!));
        services.AddTransient<KsqlDbAdminClient>();
        services.AddTransient<KsqlDbDataSourceProcessor>();
        services.AddTransient<KsqlDbDataSourceProducer>();


        services.AddHostedService<KsqlDbProducerService>();
        services.AddHostedService<KsqlDbProcessorService>();

        services.AddDbContext<IApplicationKSqlDbContext, ApplicationKSqlDbContext>(
            options =>
            {
                var setupParameters = options.UseKSqlDb(config.EndPoint!);
                setupParameters.SetAutoOffsetReset(ksqlDB.RestApi.Client.KSql.Query.Options.AutoOffsetReset.Earliest);
            }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);


        services.AddDbContextFactory<IApplicationKSqlDbContext>(factoryLifetime: ServiceLifetime.Scoped);
        services.AddTransient<IKSqlDbContextFactory, KSqlDbContextFactory>();

        return services;
    }
}