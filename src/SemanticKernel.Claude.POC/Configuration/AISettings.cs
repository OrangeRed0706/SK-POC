using SemanticKernel.Claude.POC.Abstractions;

namespace SemanticKernel.Claude.POC.Configuration;

public class AISettings
{
    public AIProviderType DefaultProvider { get; set; } = AIProviderType.Claude;
    public ClaudeSettings Claude { get; set; } = new();
    public OpenAISettings OpenAI { get; set; } = new();
    public AzureOpenAISettings AzureOpenAI { get; set; } = new();
    public GeminiSettings Gemini { get; set; } = new();
}

public class ClaudeSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-3-5-sonnet-20241022";
    public int MaxTokens { get; set; } = 1000;
}

public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 1000;
    public string? Organization { get; set; }
}

public class AzureOpenAISettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-02-01";
    public int MaxTokens { get; set; } = 1000;
}

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-pro";
    public int MaxTokens { get; set; } = 1000;
}