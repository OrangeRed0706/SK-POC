namespace SemanticKernel.Claude.POC.Abstractions;

public interface IAIProviderFactory
{
    IAIProvider GetProvider(AIProviderType providerType);
    IAIProvider GetDefaultProvider();
    IEnumerable<IAIProvider> GetAllProviders();
    IEnumerable<AIProviderType> GetAvailableProviders();
}