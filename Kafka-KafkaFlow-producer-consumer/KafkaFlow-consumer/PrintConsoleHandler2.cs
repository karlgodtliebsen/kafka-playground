using KafkaFlow;
using KafkaFlow.TypedHandler;
using KafkaFlow_Messages;

namespace Consumer;

public class PrintConsoleHandler2 : IMessageHandler<TestMessage>
{
    public Task Handle(IMessageContext context, TestMessage message)
    {
        Console.WriteLine(
            "PrintConsoleHandler2 Partition: {0} | Offset: {1} | Message: {2}",
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Text);

        return Task.CompletedTask;
    }
}