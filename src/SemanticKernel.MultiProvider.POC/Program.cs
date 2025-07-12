using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SemanticKernel.MultiProvider.POC.Configuration;
using SemanticKernel.MultiProvider.POC.Abstractions;
using SemanticKernel.MultiProvider.POC.Providers;
using SemanticKernel.MultiProvider.POC.Services;

namespace SemanticKernel.MultiProvider.POC;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Multi-AI Provider MCP Integration POC ===");
        Console.WriteLine();

        var host = CreateHostBuilder(args).Build();

        try
        {
            await RunMCPDemo(host.Services);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during execution");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("請確認您的 API Keys 已正確設定在 appsettings.json 中");
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure settings
                services.Configure<AISettings>(
                    context.Configuration.GetSection("AISettings"));

                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Register AI providers
                services.AddSingleton<ClaudeProvider>();
                services.AddSingleton<OpenAIProvider>();
                services.AddSingleton<AzureOpenAIProvider>();
                services.AddSingleton<GeminiProvider>();

                // Register factory and services
                services.AddSingleton<IAIProviderFactory, AIProviderFactory>();
                services.AddSingleton<MCPService>();
            });

    private static async Task RunMCPDemo(IServiceProvider services)
    {
        var mcpService = services.GetRequiredService<MCPService>();

        while (true)
        {
            await ShowMainMenu(mcpService);
        }
    }

    public static async Task ShowMainMenu(MCPService mcpService)
    {
        Console.WriteLine("\n=== AI Provider & MCP Integration ===");

        var availableProviders = mcpService.GetAvailableProviders().ToList();
        if (availableProviders.Any())
        {
            Console.WriteLine("可用的 AI 提供者：");
            foreach (var provider in availableProviders)
            {
                Console.WriteLine($"  • {provider}");
            }
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("⚠️  沒有可用的 AI 提供者，請檢查 API Keys 設定");
            Console.WriteLine("按任意鍵退出...");
            Console.ReadKey();
            Environment.Exit(1);
        }

        Console.WriteLine("選擇功能：");
        Console.WriteLine("1. 選擇 AI 提供者並測試");
        Console.WriteLine("2. 使用 MCP 工具呼叫");
        Console.WriteLine("3. AI 提供者 + MCP 整合示範");
        Console.WriteLine("0. 離開");
        Console.WriteLine();
        Console.Write("請選擇 (0-3): ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        try
        {
            switch (choice)
            {
                case "1":
                    await TestAIProvider(mcpService);
                    break;
                case "2":
                    await CallMCPTool(mcpService);
                    break;
                case "3":
                    await AIProviderMCPIntegration(mcpService);
                    break;
                case "0":
                    Console.WriteLine("再見!");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("無效選擇，請重試。");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"執行時發生錯誤: {ex.Message}");
        }

        Console.WriteLine("\n按任意鍵繼續...");
        Console.ReadKey();
        Console.Clear();
    }

    private static async Task TestAIProvider(MCPService mcpService)
    {
        Console.WriteLine("=== AI 提供者測試 ===");

        var providers = mcpService.GetAvailableProviderTypes().ToList();
        Console.WriteLine("選擇要測試的提供者：");
        for (int i = 0; i < providers.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {providers[i]}");
        }
        Console.Write("請選擇: ");

        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= providers.Count)
        {
            var selectedProvider = providers[choice - 1];
            Console.Write("\n請輸入測試訊息: ");
            var message = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine($"\n使用 {selectedProvider} 處理中...");
                var response = await mcpService.CallAIProvider(selectedProvider, message);
                Console.WriteLine($"\n回應: {response}");
            }
        }
        else
        {
            Console.WriteLine("無效選擇。");
        }
    }

    private static async Task CallMCPTool(MCPService mcpService)
    {
        Console.WriteLine("=== MCP 工具呼叫 ===");

        var tools = await mcpService.GetAvailableMCPTools();
        if (!tools.Any())
        {
            Console.WriteLine("目前沒有可用的 MCP 工具。");
            return;
        }

        Console.WriteLine("可用的 MCP 工具：");
        for (int i = 0; i < tools.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tools[i]}");
        }
        Console.Write("請選擇工具: ");

        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= tools.Count)
        {
            var selectedTool = tools[choice - 1];
            Console.Write("請輸入工具參數 (JSON 格式，可留空): ");
            var parameters = Console.ReadLine();

            Console.WriteLine($"\n呼叫 MCP 工具 {selectedTool}...");
            var result = await mcpService.CallMCPTool(selectedTool, parameters);
            Console.WriteLine($"\n結果: {result}");
        }
        else
        {
            Console.WriteLine("無效選擇。");
        }
    }

    private static async Task AIProviderMCPIntegration(MCPService mcpService)
    {
        Console.WriteLine("=== AI 提供者 + MCP 整合示範 ===");

        var providers = mcpService.GetAvailableProviderTypes().ToList();
        Console.WriteLine("選擇 AI 提供者：");
        for (int i = 0; i < providers.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {providers[i]}");
        }
        Console.Write("請選擇: ");

        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= providers.Count)
        {
            var selectedProvider = providers[choice - 1];
            Console.Write("\n請描述您需要什麼協助: ");
            var userRequest = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(userRequest))
            {
                Console.WriteLine($"\n使用 {selectedProvider} 與 MCP 工具協作處理中...");
                var result = await mcpService.ProcessWithAIAndMCP(selectedProvider, userRequest);
                Console.WriteLine($"\n整合結果: {result}");
            }
        }
        else
        {
            Console.WriteLine("無效選擇。");
        }
    }
}
