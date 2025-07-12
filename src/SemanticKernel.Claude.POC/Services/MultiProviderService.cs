using Microsoft.Extensions.Logging;
using SemanticKernel.Claude.POC.Abstractions;

namespace SemanticKernel.Claude.POC.Services;

public class MultiProviderService
{
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<MultiProviderService> _logger;

    public MultiProviderService(IAIProviderFactory providerFactory, ILogger<MultiProviderService> logger)
    {
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<string> ProcessWithProviderAsync(AIProviderType providerType, string userMessage)
    {
        _logger.LogInformation("Processing message with {Provider}", providerType);

        try
        {
            var provider = _providerFactory.GetProvider(providerType);
            return await provider.SendMessageAsync(userMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message with {Provider}", providerType);
            throw;
        }
    }

    public async Task<string> ProcessWithDefaultProviderAsync(string userMessage)
    {
        _logger.LogInformation("Processing message with default provider");

        try
        {
            var provider = _providerFactory.GetDefaultProvider();
            return await provider.SendMessageAsync(userMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message with default provider");
            throw;
        }
    }

    public async IAsyncEnumerable<string> ProcessWithProviderStreamAsync(AIProviderType providerType, string userMessage)
    {
        _logger.LogInformation("Processing streaming message with {Provider}", providerType);

        var provider = _providerFactory.GetProvider(providerType);
        await foreach (var chunk in provider.SendMessageStreamAsync(userMessage))
        {
            yield return chunk;
        }
    }

    public async Task<string> ProcessChatAsync(AIProviderType providerType, IEnumerable<ChatMessage> messages)
    {
        _logger.LogInformation("Processing chat with {Provider}", providerType);

        try
        {
            var provider = _providerFactory.GetProvider(providerType);
            return await provider.SendChatAsync(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat with {Provider}", providerType);
            throw;
        }
    }

    public async Task<string> ProcessIntegratedApproachAsync(string userMessage)
    {
        _logger.LogInformation("Processing message with integrated approach using multiple providers");

        try
        {
            var availableProviders = _providerFactory.GetAvailableProviders().Take(2).ToList();
            
            if (availableProviders.Count < 2)
            {
                // Fallback to single provider
                return await ProcessWithDefaultProviderAsync(userMessage);
            }

            var provider1 = _providerFactory.GetProvider(availableProviders[0]);
            var provider2 = _providerFactory.GetProvider(availableProviders[1]);

            var response1 = await provider1.SendMessageAsync(userMessage);
            
            var analysisPrompt = $@"
                分析以下 AI 回應並提供：
                1. 重點摘要
                2. 額外見解或不同觀點
                3. 改進建議
                
                原始問題：{userMessage}
                {provider1.Name} 回應：{response1}
            ";

            var response2 = await provider2.SendMessageAsync(analysisPrompt);

            return $@"
=== {provider1.Name} ({provider1.Model}) 回應 ===
{response1}

=== {provider2.Name} ({provider2.Model}) 分析 ===
{response2}
            ";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in integrated approach");
            throw;
        }
    }

    public IEnumerable<string> GetAvailableProviderNames()
    {
        return _providerFactory.GetAllProviders().Select(p => $"{p.Name} ({p.Model})");
    }

    public IEnumerable<AIProviderType> GetAvailableProviderTypes()
    {
        return _providerFactory.GetAvailableProviders();
    }
}