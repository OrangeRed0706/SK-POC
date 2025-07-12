using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SemanticKernel.Claude.POC.Abstractions;
using SemanticKernel.Claude.POC.Configuration;
using SemanticKernel.Claude.POC.Providers;

namespace SemanticKernel.Claude.POC.Services;

public class AIProviderFactory : IAIProviderFactory
{
    private readonly AISettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<AIProviderType, IAIProvider> _providers;

    public AIProviderFactory(IOptions<AISettings> settings, IServiceProvider serviceProvider)
    {
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        _providers = new Dictionary<AIProviderType, IAIProvider>();
        
        InitializeProviders();
    }

    private void InitializeProviders()
    {
        try
        {
            _providers[AIProviderType.Claude] = _serviceProvider.GetRequiredService<ClaudeProvider>();
        }
        catch (Exception)
        {
            // Claude provider initialization failed
        }

        try
        {
            _providers[AIProviderType.OpenAI] = _serviceProvider.GetRequiredService<OpenAIProvider>();
        }
        catch (Exception)
        {
            // OpenAI provider initialization failed
        }

        try
        {
            _providers[AIProviderType.AzureOpenAI] = _serviceProvider.GetRequiredService<AzureOpenAIProvider>();
        }
        catch (Exception)
        {
            // Azure OpenAI provider initialization failed
        }

        try
        {
            _providers[AIProviderType.Gemini] = _serviceProvider.GetRequiredService<GeminiProvider>();
        }
        catch (Exception)
        {
            // Gemini provider initialization failed
        }
    }

    public IAIProvider GetProvider(AIProviderType providerType)
    {
        if (_providers.TryGetValue(providerType, out var provider) && provider.IsConfigured)
        {
            return provider;
        }

        throw new InvalidOperationException($"Provider {providerType} is not available or not configured");
    }

    public IAIProvider GetDefaultProvider()
    {
        // Try to get the configured default provider
        if (_providers.TryGetValue(_settings.DefaultProvider, out var defaultProvider) && defaultProvider.IsConfigured)
        {
            return defaultProvider;
        }

        // Fallback to any configured provider
        var configuredProvider = _providers.Values.FirstOrDefault(p => p.IsConfigured);
        if (configuredProvider != null)
        {
            return configuredProvider;
        }

        throw new InvalidOperationException("No AI providers are configured");
    }

    public IEnumerable<IAIProvider> GetAllProviders()
    {
        return _providers.Values.Where(p => p.IsConfigured);
    }

    public IEnumerable<AIProviderType> GetAvailableProviders()
    {
        return _providers
            .Where(kvp => kvp.Value.IsConfigured)
            .Select(kvp => kvp.Key);
    }
}