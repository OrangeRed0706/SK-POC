using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using SemanticKernel.Claude.POC.Models;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace SemanticKernel.Claude.POC.Examples;

public class ClaudeIntegrationExamples
{
    private readonly AnthropicClient _anthropicClient;
    private readonly Kernel _kernel;
    private readonly AnthropicSettings _settings;
    private readonly ILogger<ClaudeIntegrationExamples> _logger;

    public ClaudeIntegrationExamples(
        IOptions<AnthropicSettings> settings,
        ILogger<ClaudeIntegrationExamples> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _anthropicClient = new AnthropicClient(_settings.ApiKey);
        
        var builder = Kernel.CreateBuilder();
        _kernel = builder.Build();
    }

    public async Task RunBasicClaudeExampleAsync()
    {
        Console.WriteLine("=== Basic Claude API Example ===");
        
        var messages = new List<Message>
        {
            new Message(RoleType.User, "Explain what .NET Semantic Kernel is in 3 sentences.")
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = 500,
            Stream = false
        };

        var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);
        var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
        Console.WriteLine($"Claude Response: {textContent?.Text ?? "No response"}");
        Console.WriteLine();
    }

    public async Task RunSemanticKernelPluginExampleAsync()
    {
        Console.WriteLine("=== Semantic Kernel Plugin Example ===");
        
        var claudePlugin = _kernel.CreatePluginFromType<ClaudePlugin>();
        _kernel.Plugins.Add(claudePlugin);

        var function = _kernel.CreateFunctionFromPrompt(
            @"Use the Claude plugin to get information about {{$topic}} and then summarize it in bullet points.
            
            Topic: {{$topic}}");

        var result = await _kernel.InvokeAsync(function, new() { ["topic"] = "machine learning" });
        Console.WriteLine($"Semantic Kernel with Claude Plugin: {result}");
        Console.WriteLine();
    }

    public async Task RunConversationChainExampleAsync()
    {
        Console.WriteLine("=== Conversation Chain Example ===");
        
        var conversationHistory = new List<Message>();
        
        var topics = new[] { "What is AI?", "How does machine learning work?", "What are the benefits of AI?" };
        
        foreach (var topic in topics)
        {
            conversationHistory.Add(new Message(RoleType.User, topic));
            
            var parameters = new MessageParameters
            {
                Messages = conversationHistory,
                Model = _settings.Model,
                MaxTokens = 300,
                Stream = false
            };

            var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);
            var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
            var responseText = textContent?.Text ?? "";
            conversationHistory.Add(new Message(RoleType.Assistant, responseText));
            
            Console.WriteLine($"Q: {topic}");
            Console.WriteLine($"A: {responseText}");
            Console.WriteLine();
        }
    }

    public async Task RunFunctionCallingExampleAsync()
    {
        Console.WriteLine("=== Function Calling Example ===");
        
        var weatherFunction = _kernel.CreateFunctionFromMethod(
            (string location) => $"The weather in {location} is sunny and 72°F",
            "GetWeather",
            "Gets the current weather for a location");


        var result = await _kernel.InvokeAsync(weatherFunction, new() { ["location"] = "New York City" });
        Console.WriteLine($"Weather Function Result: {result}");
        
        var claudeResponse = await ProcessWeatherWithClaudeAsync("New York City", result.ToString());
        Console.WriteLine($"Claude Enhanced Response: {claudeResponse}");
        Console.WriteLine();
    }

    private async Task<string> ProcessWeatherWithClaudeAsync(string location, string weatherData)
    {
        var messages = new List<Message>
        {
            new Message(RoleType.User, $"I got this weather data for {location}: {weatherData}. Can you provide a nice summary with some recommendations for activities?")
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = 300,
            Stream = false
        };

        var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);
        var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
        return textContent?.Text ?? "";
    }
}

public class ClaudePlugin
{
    private readonly AnthropicClient _anthropicClient;
    private readonly AnthropicSettings _settings;

    public ClaudePlugin(IOptions<AnthropicSettings> settings)
    {
        _settings = settings.Value;
        _anthropicClient = new AnthropicClient(_settings.ApiKey);
    }

    [KernelFunction("ask_claude")]
    [Description("Ask Claude AI a question")]
    public async Task<string> AskClaudeAsync(
        [Description("The question to ask Claude")] string question)
    {
        var messages = new List<Message>
        {
            new Message(RoleType.User, question)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = _settings.Model,
            MaxTokens = 500,
            Stream = false
        };

        var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);
        var textContent = response.Content.OfType<Anthropic.SDK.Messaging.TextContent>().FirstOrDefault();
        return textContent?.Text ?? "No response received";
    }
}