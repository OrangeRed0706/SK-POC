using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using SemanticKernel.Claude.POC.Models;
using System.Runtime.CompilerServices;

namespace SemanticKernel.Claude.POC.Services;

public class ClaudeChatCompletionService : IChatCompletionService
{
    private readonly AnthropicSettings _settings;
    private readonly AnthropicClient _anthropicClient;

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public ClaudeChatCompletionService(AnthropicSettings settings, AnthropicClient anthropicClient)
    {
        _settings = settings;
        _anthropicClient = anthropicClient;
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var messages = ConvertChatHistoryToMessages(chatHistory);
        
        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = 1000,
            Stream = false
        };

        var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);
        var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
        var responseText = textContent?.Text ?? "No response received";

        var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, responseText);
        return new List<ChatMessageContent> { chatMessageContent };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = ConvertChatHistoryToMessages(chatHistory);
        
        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = 1000,
            Stream = true
        };

        await foreach (var result in _anthropicClient.Messages.StreamClaudeMessageAsync(parameters))
        {
            if (result.Delta?.Text != null)
            {
                yield return new StreamingChatMessageContent(AuthorRole.Assistant, result.Delta.Text);
            }
        }
    }

    private List<Message> ConvertChatHistoryToMessages(ChatHistory chatHistory)
    {
        var messages = new List<Message>();
        
        foreach (var message in chatHistory)
        {
            var role = message.Role == AuthorRole.User ? RoleType.User : RoleType.Assistant;
            messages.Add(new Message(role, message.Content ?? string.Empty));
        }

        return messages;
    }
}