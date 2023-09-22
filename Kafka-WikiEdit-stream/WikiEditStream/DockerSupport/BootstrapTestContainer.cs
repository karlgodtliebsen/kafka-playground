using Microsoft.Extensions.Options;
using WikiEditStream.Configuration;
using WikiEditStream.DockerSupport;

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
            await dockerComposeBuilder.BuildAndStartConfluentDocker();
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