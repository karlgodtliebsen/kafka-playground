using KafkaFlow;
using KafkaFlow_Messages;

namespace Consumer;

class PrintConsoleMiddleware2 : IMessageMiddleware
{
    public Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        TestMessage? message = context.Message.Value as TestMessage;
        Console.WriteLine($"Consumed 2: {message!.Text}");
        return Task.CompletedTask;
    }
}