namespace SemanticKernel.Claude.POC.Abstractions;

public interface IAIProvider
{
    string Name { get; }
    string Model { get; }
    bool IsConfigured { get; }
    
    Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> SendMessageStreamAsync(string message, CancellationToken cancellationToken = default);
    Task<string> SendChatAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> SendChatStreamAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
}

public record ChatMessage(string Role, string Content);

public enum AIProviderType
{
    Claude,
    OpenAI,
    AzureOpenAI,
    Gemini
}