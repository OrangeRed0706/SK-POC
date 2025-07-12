using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernel.Claude.POC.Abstractions;
using SemanticKernel.Claude.POC.Configuration;
using System.Runtime.CompilerServices;

namespace SemanticKernel.Claude.POC.Providers;

public class AzureOpenAIProvider : AIProviderBase
{
    private readonly AzureOpenAISettings _settings;
    private readonly IChatCompletionService _chatService;

    public AzureOpenAIProvider(IOptions<AISettings> settings, ILogger<AzureOpenAIProvider> logger) 
        : base(logger)
    {
        _settings = settings.Value.AzureOpenAI;
        
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: _settings.DeploymentName,
            endpoint: _settings.Endpoint,
            apiKey: _settings.ApiKey,
            apiVersion: _settings.ApiVersion
        );
        
        var kernel = kernelBuilder.Build();
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public override string Name => "Azure OpenAI";
    public override string Model => _settings.DeploymentName;
    public override bool IsConfigured => !string.IsNullOrEmpty(_settings.ApiKey) && !string.IsNullOrEmpty(_settings.Endpoint);

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
            LogError(ex, "Failed to send message to Azure OpenAI");
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