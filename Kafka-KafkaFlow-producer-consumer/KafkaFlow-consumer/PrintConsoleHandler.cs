using KafkaFlow;
using KafkaFlow.TypedHandler;

using KafkaFlow_Messages;

namespace Consumer;

public class PrintConsoleHandler1 : IMessageHandler<TestMessage>
{
    public Task Handle(IMessageContext context, TestMessage message)
    {
        Console.WriteLine(
            "PrintConsoleHandler1 Partition: {0} | Offset: {1} | Message: {2}",
            context.ConsumerContext.Partition,
            context.ConsumerContext.Offset,
            message.Text);

        return Task.CompletedTask;
    }
}
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

class PrintConsoleMiddleware1 : IMessageMiddleware
{
    public Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        TestMessage? message = context.Message.Value as TestMessage;
        Console.WriteLine($"Consumed 1: {message!.Text}");
        return Task.CompletedTask;
    }
}

class PrintConsoleMiddleware2 : IMessageMiddleware
{
    public Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        TestMessage? message = context.Message.Value as TestMessage;
        Console.WriteLine($"Consumed 2: {message!.Text}");
        return Task.CompletedTask;
    }
}