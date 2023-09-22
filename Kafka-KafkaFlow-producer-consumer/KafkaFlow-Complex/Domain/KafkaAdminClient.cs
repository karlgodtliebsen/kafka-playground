using Confluent.Kafka;
using Confluent.Kafka.Admin;

using KsqlDb.Configuration;

using Microsoft.Extensions.Options;

namespace KsqlDb.Domain;

public class KafkaAdminClient
{
    private readonly KsqlDbConfiguration config;

    public KafkaAdminClient(IOptions<KsqlDbConfiguration> options)
    {
        config = options.Value;
    }

    public async Task CreateTopic(string topicName)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        Console.WriteLine($"{nameof(CreateTopic)} starting");
        // Configure the admin client based on the provided configuration. 
        var adminConfig = new AdminClientConfig(config);

        // Build an admin client that uses the provided configuration.
        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        try
        {
            // Create a topic with the specified name, three partitions, and a single replica.
            await adminClient.CreateTopicsAsync(new TopicSpecification[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            });
            Console.WriteLine($"Created topic {topicName}");
        }
        catch (CreateTopicsException ex)
        {
            Console.WriteLine($"An error occurred creating topic {topicName}: {ex.Message}");
        }
    }

    public async Task DeleteTopic(string topicName)
    {
        Console.WriteLine($"{nameof(DeleteTopic)} starting");

        // Configure the admin client based on the provided configuration. 
        var adminConfig = new AdminClientConfig(config);

        // Build an admin client that uses the provided configuration.
        using var adminClient = new AdminClientBuilder(adminConfig).Build();

        try
        {
            // Delete the topic with the specified name.
            await adminClient.DeleteTopicsAsync(new string[] { topicName });
            Console.WriteLine($"Deleted topic {topicName}");
        }
        catch (DeleteTopicsException ex)
        {
            Console.WriteLine($"An error occurred deleting topic {topicName}: {ex.Message}");
        }
    }
}