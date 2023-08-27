using Microsoft.Extensions.Options;
using WikiEditStream;
using WikiEditStream.Configuration;

public class BootstrapTestContainer
{
    private readonly KafkaConfiguration configuration;
    private readonly ConfluentKafkaDockerComposeBuilder dockerComposeBuilder;

    public BootstrapTestContainer(ConfluentKafkaDockerComposeBuilder dockerComposeBuilder, IOptions<KafkaConfiguration> options)
    {
        this.dockerComposeBuilder = dockerComposeBuilder;
        this.configuration = options.Value;
    }

    public async Task Start()
    {
        if (configuration.UserDockerTestContainer)
        {
            await dockerComposeBuilder.BuildConfluentDocker();
            Console.WriteLine(dockerComposeBuilder.PortData());
        }
    }

    public async Task Stop()
    {
        if (configuration.UserDockerTestContainer)
        {
            await dockerComposeBuilder.DisposeAsync();
        }
    }

}