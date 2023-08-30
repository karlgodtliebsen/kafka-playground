using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using KsqlDb.Domain;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Microsoft.Extensions.Options;
using KsqlDb.DbContext;
using KsqlDb.HostServices;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.Logging;

namespace KsqlDb.Configuration;

public static class KsqlDbConfigurator
{

    public static IServiceCollection AddKsqlDb(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection(KsqlDbConfiguration.SectionName).Get<KsqlDbConfiguration>()!;
        services.AddSingleton(Options.Create(config!));
        services.AddTransient<KsqlDbAdminClient>();
        services.AddTransient<KsqlDbProcessor>();
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

        //services.AddHttpClient<IKSqlDbRestApiClient, KSqlDbRestApiClient>((sp, client) =>
        //    {
        //        var cfg = sp.GetRequiredService<IOptions<KsqlDbConfiguration>>().Value;
        //        client.BaseAddress = new Uri(cfg.EndPoint!);
        //    })
        //    .AddPolicyHandler(HttpClientsPolicies.GetCircuitBreakerPolicyForNotFound())
        //    .AddPolicyHandler(HttpClientsPolicies.GetRetryPolicy());
        return services;
    }
}