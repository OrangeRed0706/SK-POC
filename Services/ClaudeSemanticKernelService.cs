using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using SemanticKernel.Claude.POC.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Claude.POC.Services;

public class ClaudeSemanticKernelService
{
    private readonly Kernel _kernel;
    private readonly AnthropicClient _anthropicClient;
    private readonly AnthropicSettings _settings;
    private readonly ILogger<ClaudeSemanticKernelService> _logger;

    public ClaudeSemanticKernelService(
        IOptions<AnthropicSettings> settings,
        ILogger<ClaudeSemanticKernelService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        
        _anthropicClient = new AnthropicClient(_settings.ApiKey);
        
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<IChatCompletionService>(sp => new ClaudeChatCompletionService(_settings, _anthropicClient));
        _kernel = builder.Build();
    }

    public async Task<string> ProcessWithSemanticKernelAsync(string userMessage)
    {
        var function = _kernel.CreateFunctionFromPrompt(
            @"You are a helpful AI assistant. Please analyze the user's message and provide a thoughtful response.
            
            User message: {{$input}}
            
            Please provide a response that is helpful, accurate, and engaging.");

        var result = await _kernel.InvokeAsync(function, new() { ["input"] = userMessage });
        
        return result.ToString();
    }

    public async Task<string> ProcessWithClaudeDirectAsync(string userMessage)
    {
        var messages = new List<Message>
        {
            new Message(RoleType.User, userMessage)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = 1000,
            Stream = false
        };

        var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);
        
        var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
        return textContent?.Text ?? "No response received";
    }

    public async Task<string> ProcessWithIntegratedApproachAsync(string userMessage)
    {
        _logger.LogInformation("Processing message with integrated approach: {Message}", userMessage);

        var claudeResponse = await ProcessWithClaudeDirectAsync(userMessage);
        
        var semanticKernelPrompt = $@"
        I received this response from Claude AI: '{claudeResponse}'
        
        Please analyze this response and provide:
        1. A summary of the key points
        2. Any additional insights or perspectives
        3. Suggestions for follow-up questions
        
        Original user message was: '{userMessage}'";

        var skResponse = await ProcessWithSemanticKernelAsync(semanticKernelPrompt);
        
        return $"=== Claude Direct Response ===\n{claudeResponse}\n\n=== Semantic Kernel Analysis ===\n{skResponse}";
    }

    public async Task<string> ProcessWithStreamingAsync(string userMessage)
    {
        var messages = new List<Message>
        {
            new Message(RoleType.User, userMessage)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = 1000,
            Stream = true
        };

        var response = string.Empty;
        await foreach (var result in _anthropicClient.Messages.StreamClaudeMessageAsync(parameters))
        {
            if (result.Delta?.Text != null)
            {
                response += result.Delta.Text;
                Console.Write(result.Delta.Text);
            }
        }
        Console.WriteLine();
        
        return response;
    }
}