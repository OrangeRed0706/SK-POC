using Microsoft.Extensions.Logging;

namespace SemanticKernel.Claude.POC.Abstractions;

public abstract class AIProviderBase : IAIProvider
{
    protected readonly ILogger Logger;

    protected AIProviderBase(ILogger logger)
    {
        Logger = logger;
    }

    public abstract string Name { get; }
    public abstract string Model { get; }
    public abstract bool IsConfigured { get; }

    public abstract Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default);
    public abstract IAsyncEnumerable<string> SendMessageStreamAsync(string message, CancellationToken cancellationToken = default);
    public abstract Task<string> SendChatAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
    public abstract IAsyncEnumerable<string> SendChatStreamAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);

    protected virtual void LogRequest(string message)
    {
        Logger.LogDebug("[{Provider}] Sending request: {Message}", Name, message.Length > 100 ? message[..100] + "..." : message);
    }

    protected virtual void LogResponse(string response)
    {
        Logger.LogDebug("[{Provider}] Received response: {Response}", Name, response.Length > 100 ? response[..100] + "..." : response);
    }

    protected virtual void LogError(Exception exception, string message)
    {
        Logger.LogError(exception, "[{Provider}] {Message}", Name, message);
    }
}