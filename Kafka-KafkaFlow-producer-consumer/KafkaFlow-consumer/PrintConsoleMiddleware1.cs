using KafkaFlow;
using KafkaFlow_Messages;

namespace Consumer;

class PrintConsoleMiddleware1 : IMessageMiddleware
{
    public Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        TestMessage? message = context.Message.Value as TestMessage;
        Console.WriteLine($"Consumed 1: {message!.Text}");
        return Task.CompletedTask;
    }
}