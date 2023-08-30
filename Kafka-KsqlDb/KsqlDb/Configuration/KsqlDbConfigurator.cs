using ksqlDb.RestApi.Client.DependencyInjection;

using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;

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
        services.AddTransient<KsqlDbTweetProcessor>();
        services.AddHostedService<KsqlDbProcessorService>();

        services.AddDbContext<IApplicationKSqlDbContext, ApplicationKSqlDbContext>(
            options =>
            {
                var setupParameters = options.UseKSqlDb(config.EndPoint!);
                setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);
            }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

        services.AddDbContextFactory<IApplicationKSqlDbContext>(factoryLifetime: ServiceLifetime.Scoped);
        services.AddTransient<IKSqlDbContextFactory, KSqlDbContextFactory>();

        services.AddTransient<ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory>((sp) =>
        {
            var cfg = sp.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(cfg.EndPoint!)
            };
            return new HttpClientFactory(httpClient);
        });

        services.AddTransient<IKSqlDbRestApiClient, KSqlDbRestApiClient>();
        return services;
    }
}