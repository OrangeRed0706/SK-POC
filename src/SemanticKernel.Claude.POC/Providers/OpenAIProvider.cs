using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernel.Claude.POC.Abstractions;
using SemanticKernel.Claude.POC.Configuration;
using System.Runtime.CompilerServices;

namespace SemanticKernel.Claude.POC.Providers;

public class OpenAIProvider : AIProviderBase
{
    private readonly OpenAISettings _settings;
    private readonly IChatCompletionService _chatService;

    public OpenAIProvider(IOptions<AISettings> settings, ILogger<OpenAIProvider> logger) 
        : base(logger)
    {
        _settings = settings.Value.OpenAI;
        
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: _settings.Model,
            apiKey: _settings.ApiKey,
            orgId: _settings.Organization
        );
        
        var kernel = kernelBuilder.Build();
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public override string Name => "OpenAI";
    public override string Model => _settings.Model;
    public override bool IsConfigured => !string.IsNullOrEmpty(_settings.ApiKey);

    public override async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        LogRequest(message);

        try
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(message);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = _settings.MaxTokens
            };

            var result = await _chatService.GetChatMessageContentAsync(
                chatHistory, 
                executionSettings, 
                cancellationToken: cancellationToken);

            var response = result.Content ?? "No response received";
            LogResponse(response);
            return response;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to send message to OpenAI");
            throw;
        }
    }

    public override async IAsyncEnumerable<string> SendMessageStreamAsync(
        string message, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LogRequest(message);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(message);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = _settings.MaxTokens
        };

        await foreach (var result in _chatService.GetStreamingChatMessageContentsAsync(
            chatHistory, 
            executionSettings, 
            cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(result.Content))
            {
                yield return result.Content;
            }
        }
    }

    public override async Task<string> SendChatAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory();
        foreach (var message in messages)
        {
            if (message.Role.ToLowerInvariant() == "user")
            {
                chatHistory.AddUserMessage(message.Content);
            }
            else
            {
                chatHistory.AddAssistantMessage(message.Content);
            }
        }

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = _settings.MaxTokens
        };

        var result = await _chatService.GetChatMessageContentAsync(
            chatHistory, 
            executionSettings, 
            cancellationToken: cancellationToken);

        return result.Content ?? "No response received";
    }

    public override async IAsyncEnumerable<string> SendChatStreamAsync(
        IEnumerable<ChatMessage> messages, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory();
        foreach (var message in messages)
        {
            if (message.Role.ToLowerInvariant() == "user")
            {
                chatHistory.AddUserMessage(message.Content);
            }
            else
            {
                chatHistory.AddAssistantMessage(message.Content);
            }
        }

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = _settings.MaxTokens
        };

        await foreach (var result in _chatService.GetStreamingChatMessageContentsAsync(
            chatHistory, 
            executionSettings, 
            cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(result.Content))
            {
                yield return result.Content;
            }
        }
    }
}