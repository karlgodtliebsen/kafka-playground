using Microsoft.Extensions.Options;

using WikiEditStream.Configuration;

namespace WikiEditStream.Domain;

public class KafkaKSql
{
    /// <summary>
    /// https://github.com/confluentinc/confluent-kafka-dotnet/issues/344
    /// https://github.com/LGouellec/kafka-streams-dotnet
    /// https://github.com/confluentinc/ksql/issues/734
    /// </summary>
    private readonly KafkaConfiguration config;

    public KafkaKSql(IOptions<KafkaConfiguration> options)
    {
        config = options.Value;
    }
}