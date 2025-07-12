using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SemanticKernel.MultiProvider.POC.Abstractions;
using SemanticKernel.MultiProvider.POC.Configuration;
using System.Runtime.CompilerServices;

namespace SemanticKernel.MultiProvider.POC.Providers;

public class ClaudeProvider : AIProviderBase
{
    private readonly ClaudeSettings _settings;
    private readonly AnthropicClient _client;

    public ClaudeProvider(IOptions<AISettings> settings, ILogger<ClaudeProvider> logger) 
        : base(logger)
    {
        _settings = settings.Value.Claude;
        _client = new AnthropicClient(_settings.ApiKey);
    }

    public override string Name => "Claude";
    public override string Model => _settings.Model;
    public override bool IsConfigured => !string.IsNullOrEmpty(_settings.ApiKey);

    public override async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        LogRequest(message);

        try
        {
            var messages = new List<Message>
            {
                new Message(RoleType.User, message)
            };

            var parameters = new MessageParameters
            {
                Messages = messages,
                Model = _settings.Model,
                MaxTokens = _settings.MaxTokens,
                Stream = false
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters);
            var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
            var result = textContent?.Text ?? "No response received";

            LogResponse(result);
            return result;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to send message to Claude");
            throw;
        }
    }

    public override async IAsyncEnumerable<string> SendMessageStreamAsync(
        string message, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LogRequest(message);

        var messages = new List<Message>
        {
            new Message(RoleType.User, message)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = _settings.MaxTokens,
            Stream = true
        };

        await foreach (var result in _client.Messages.StreamClaudeMessageAsync(parameters))
        {
            if (result.Delta?.Text != null)
            {
                yield return result.Delta.Text;
            }
        }
    }

    public override async Task<string> SendChatAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var claudeMessages = ConvertToClaude(messages);
        
        var parameters = new MessageParameters
        {
            Messages = claudeMessages,
            Model = _settings.Model,
            MaxTokens = _settings.MaxTokens,
            Stream = false
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters);
        var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
        return textContent?.Text ?? "No response received";
    }

    public override async IAsyncEnumerable<string> SendChatStreamAsync(
        IEnumerable<ChatMessage> messages, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var claudeMessages = ConvertToClaude(messages);
        
        var parameters = new MessageParameters
        {
            Messages = claudeMessages,
            Model = _settings.Model,
            MaxTokens = _settings.MaxTokens,
            Stream = true
        };

        await foreach (var result in _client.Messages.StreamClaudeMessageAsync(parameters))
        {
            if (result.Delta?.Text != null)
            {
                yield return result.Delta.Text;
            }
        }
    }

    private List<Message> ConvertToClaude(IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => new Message(
            m.Role.ToLowerInvariant() switch
            {
                "user" => RoleType.User,
                "assistant" => RoleType.Assistant,
                _ => RoleType.User
            },
            m.Content
        )).ToList();
    }
}