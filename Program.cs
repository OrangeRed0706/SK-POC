using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SemanticKernel.Claude.POC.Models;
using SemanticKernel.Claude.POC.Services;
using SemanticKernel.Claude.POC.Examples;

namespace SemanticKernel.Claude.POC;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== .NET Semantic Kernel + Claude C# SDK POC ===");
        Console.WriteLine();

        var host = CreateHostBuilder(args).Build();

        try
        {
            await RunDemoAsync(host.Services);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during execution");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("請確認您的 API Key 已正確設定在 appsettings.json 中");
        }
    }

        static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AnthropicSettings>(
                    context.Configuration.GetSection("AnthropicSettings"));
                
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                services.AddSingleton<ClaudeSemanticKernelService>();
                services.AddSingleton<ClaudeIntegrationExamples>();
            });

    static async Task RunDemoAsync(IServiceProvider services)
    {
        var mainService = services.GetRequiredService<ClaudeSemanticKernelService>();
        var examples = services.GetRequiredService<ClaudeIntegrationExamples>();

        await RunInteractiveDemo(mainService, examples);
    }

    static async Task RunInteractiveDemo(ClaudeSemanticKernelService service, ClaudeIntegrationExamples examples)
    {
        while (true)
        {
            Console.WriteLine("\n選擇示範類型:");
            Console.WriteLine("1. 基本 Claude API 呼叫");
            Console.WriteLine("2. Semantic Kernel 基本功能");
            Console.WriteLine("3. 整合式處理 (Claude + SK)");
            Console.WriteLine("4. 串流回應");
            Console.WriteLine("5. 對話鏈範例");
            Console.WriteLine("6. 函數呼叫範例");
            Console.WriteLine("7. 自訂訊息測試");
            Console.WriteLine("0. 離開");
            Console.WriteLine();
            Console.Write("請選擇 (0-7): ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await examples.RunBasicClaudeExampleAsync();
                        break;
                    case "2":
                        await RunSemanticKernelDemo(service);
                        break;
                    case "3":
                        await RunIntegratedDemo(service);
                        break;
                    case "4":
                        await RunStreamingDemo(service);
                        break;
                    case "5":
                        await examples.RunConversationChainExampleAsync();
                        break;
                    case "6":
                        await examples.RunFunctionCallingExampleAsync();
                        break;
                    case "7":
                        await RunCustomMessageDemo(service);
                        break;
                    case "0":
                        Console.WriteLine("再見!");
                        return;
                    default:
                        Console.WriteLine("無效選擇，請重試。");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"執行時發生錯誤: {ex.Message}");
                if (ex.Message.Contains("API key"))
                {
                    Console.WriteLine("請檢查您的 Anthropic API Key 設定。");
                }
            }

            Console.WriteLine("\n按任意鍵繼續...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    static async Task RunSemanticKernelDemo(ClaudeSemanticKernelService service)
    {
        Console.WriteLine("=== Semantic Kernel 基本功能展示 ===");
        var testMessage = "請解釋什麼是人工智慧，並給出3個實際應用例子";
        
        Console.WriteLine($"測試訊息: {testMessage}");
        Console.WriteLine("處理中...");
        
        var result = await service.ProcessWithSemanticKernelAsync(testMessage);
        Console.WriteLine($"結果: {result}");
    }

    static async Task RunIntegratedDemo(ClaudeSemanticKernelService service)
    {
        Console.WriteLine("=== 整合式處理展示 ===");
        var testMessage = "什麼是機器學習？它如何改變我們的生活？";
        
        Console.WriteLine($"測試訊息: {testMessage}");
        Console.WriteLine("處理中...");
        
        var result = await service.ProcessWithIntegratedApproachAsync(testMessage);
        Console.WriteLine(result);
    }

    static async Task RunStreamingDemo(ClaudeSemanticKernelService service)
    {
        Console.WriteLine("=== 串流回應展示 ===");
        var testMessage = "請寫一首關於程式設計的短詩";
        
        Console.WriteLine($"測試訊息: {testMessage}");
        Console.WriteLine("串流回應:");
        Console.WriteLine("---");
        
        await service.ProcessWithStreamingAsync(testMessage);
        
        Console.WriteLine("---");
    }

    static async Task RunCustomMessageDemo(ClaudeSemanticKernelService service)
    {
        Console.WriteLine("=== 自訂訊息測試 ===");
        Console.Write("請輸入您的訊息: ");
        var userMessage = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            Console.WriteLine("訊息不能為空。");
            return;
        }

        Console.WriteLine("\n選擇處理方式:");
        Console.WriteLine("1. 僅使用 Claude API");
        Console.WriteLine("2. 僅使用 Semantic Kernel");
        Console.WriteLine("3. 整合式處理");
        Console.WriteLine("4. 串流回應");
        Console.Write("請選擇 (1-4): ");

        var method = Console.ReadLine();
        Console.WriteLine("\n處理中...");

        switch (method)
        {
            case "1":
                var claudeResult = await service.ProcessWithClaudeDirectAsync(userMessage);
                Console.WriteLine($"Claude 回應: {claudeResult}");
                break;
            case "2":
                var skResult = await service.ProcessWithSemanticKernelAsync(userMessage);
                Console.WriteLine($"Semantic Kernel 回應: {skResult}");
                break;
            case "3":
                var integratedResult = await service.ProcessWithIntegratedApproachAsync(userMessage);
                Console.WriteLine(integratedResult);
                break;
            case "4":
                Console.WriteLine("串流回應:");
                Console.WriteLine("---");
                await service.ProcessWithStreamingAsync(userMessage);
                Console.WriteLine("---");
                break;
            default:
                Console.WriteLine("無效選擇。");
                break;
        }
    }
}