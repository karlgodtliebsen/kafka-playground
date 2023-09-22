using Microsoft.Extensions.Options;

using WikiEditStream.Configuration;
using WikiEditStream.DockerSupport;

namespace WikiEditStream.Tests
{
    public class TestOfBuildingConfluentKafka : IAsyncLifetime
    {
        private ConfluentKafkaDockerComposeBuilder kafkaDockerComposeBuilder;

        public TestOfBuildingConfluentKafka()
        {
            IOptions<KafkaConfiguration> options = Options.Create(new KafkaConfiguration());
            kafkaDockerComposeBuilder = new ConfluentKafkaDockerComposeBuilder(options);
        }

        public async Task InitializeAsync()
        {
          //Is started by builder in order to get port mappings
        }

        public async Task DisposeAsync()
        {
            await kafkaDockerComposeBuilder.DisposeAsync();
        }


        [Fact]
        public async Task TestOfConfluentKafkaSetup()
        {
            await kafkaDockerComposeBuilder.BuildAndStartConfluentDocker();
        }
    }
}