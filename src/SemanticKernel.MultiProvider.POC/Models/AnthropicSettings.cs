namespace SemanticKernel.MultiProvider.POC.Models;

public class AnthropicSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-3-5-sonnet-20241022";
}