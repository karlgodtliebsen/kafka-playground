using KafkaFlow;

namespace FlowControl;

class PrintConsoleMiddleware : IMessageMiddleware
{
    public Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var msg = System.Text.Json.JsonSerializer.Serialize(context.Message.Value);
        Console.WriteLine($"Message: {msg}");
        return Task.CompletedTask;
    }
}