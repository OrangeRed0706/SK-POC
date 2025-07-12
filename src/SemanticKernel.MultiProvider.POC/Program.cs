using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SemanticKernel.MultiProvider.POC.Configuration;
using SemanticKernel.MultiProvider.POC.Services;
using SemanticKernel.MultiProvider.POC.Examples;
using SemanticKernel.MultiProvider.POC.Abstractions;
using SemanticKernel.MultiProvider.POC.Providers;

namespace SemanticKernel.MultiProvider.POC;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Multi-AI Provider Semantic Kernel POC ===");
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
                services.AddSingleton<MultiProviderService>();
                services.AddSingleton<ClaudeIntegrationExamples>();
            });

    static async Task RunDemoAsync(IServiceProvider services)
    {
        var multiProviderService = services.GetRequiredService<MultiProviderService>();
        var examples = services.GetRequiredService<ClaudeIntegrationExamples>();

        await RunInteractiveDemo(multiProviderService, examples);
    }

    static async Task RunInteractiveDemo(MultiProviderService service, ClaudeIntegrationExamples examples)
    {
        while (true)
        {
            Console.WriteLine("\n=== 多 AI 提供者展示 ===");
            
            var availableProviders = service.GetAvailableProviderNames().ToList();
            if (availableProviders.Any())
            {
                Console.WriteLine("可用的 AI 提供者：");
                foreach (var provider in availableProviders)
                {
                    Console.WriteLine($"  • {provider}");
                }
                Console.WriteLine();
            }
            
            Console.WriteLine("選擇示範類型:");
            Console.WriteLine("1. 單一提供者測試");
            Console.WriteLine("2. 多提供者比較");
            Console.WriteLine("3. 整合式處理 (多提供者協作)");
            Console.WriteLine("4. 串流回應測試");
            Console.WriteLine("5. 自訂訊息測試");
            Console.WriteLine("0. 離開");
            Console.WriteLine();
            Console.Write("請選擇 (0-5): ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await RunSingleProviderDemo(service);
                        break;
                    case "2":
                        await RunMultiProviderComparison(service);
                        break;
                    case "3":
                        await RunIntegratedDemo(service);
                        break;
                    case "4":
                        await RunStreamingDemo(service);
                        break;
                    case "5":
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

    static async Task RunSingleProviderDemo(MultiProviderService service)
    {
        Console.WriteLine("=== 單一提供者測試 ===");
        
        var availableProviders = service.GetAvailableProviderTypes().ToList();
        if (!availableProviders.Any())
        {
            Console.WriteLine("沒有可用的 AI 提供者。請檢查設定檔。");
            return;
        }

        Console.WriteLine("選擇要測試的提供者：");
        for (int i = 0; i < availableProviders.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {availableProviders[i]}");
        }
        Console.Write("請選擇: ");

        if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= availableProviders.Count)
        {
            var selectedProvider = availableProviders[choice - 1];
            var testMessage = "請解釋什麼是人工智慧，並給出3個實際應用例子";

            Console.WriteLine($"\n使用 {selectedProvider} 處理訊息...");
            Console.WriteLine($"測試訊息: {testMessage}");
            Console.WriteLine("處理中...\n");

            var result = await service.ProcessWithProviderAsync(selectedProvider, testMessage);
            Console.WriteLine($"結果: {result}");
        }
        else
        {
            Console.WriteLine("無效選擇。");
        }
    }

    static async Task RunMultiProviderComparison(MultiProviderService service)
    {
        Console.WriteLine("=== 多提供者比較 ===");
        
        var availableProviders = service.GetAvailableProviderTypes().ToList();
        if (availableProviders.Count < 2)
        {
            Console.WriteLine("需要至少2個已設定的提供者進行比較。");
            return;
        }

        var testMessage = "什麼是機器學習？請用簡單的詞彙解釋。";
        Console.WriteLine($"測試訊息: {testMessage}");
        Console.WriteLine("\n開始比較不同提供者的回應...\n");

        foreach (var provider in availableProviders.Take(3)) // 限制最多3個提供者
        {
            try
            {
                Console.WriteLine($"=== {provider} ===");
                var result = await service.ProcessWithProviderAsync(provider, testMessage);
                Console.WriteLine(result);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{provider} 發生錯誤: {ex.Message}");
                Console.WriteLine();
            }
        }
    }

    static async Task RunIntegratedDemo(MultiProviderService service)
    {
        Console.WriteLine("=== 整合式處理展示 (多提供者協作) ===");
        var testMessage = "什麼是量子計算？它的未來發展如何？";
        
        Console.WriteLine($"測試訊息: {testMessage}");
        Console.WriteLine("處理中...\n");
        
        var result = await service.ProcessIntegratedApproachAsync(testMessage);
        Console.WriteLine(result);
    }

    static async Task RunStreamingDemo(MultiProviderService service)
    {
        Console.WriteLine("=== 串流回應測試 ===");
        
        var availableProviders = service.GetAvailableProviderTypes().ToList();
        if (!availableProviders.Any())
        {
            Console.WriteLine("沒有可用的 AI 提供者。");
            return;
        }

        var provider = availableProviders.First();
        var testMessage = "請寫一首關於人工智慧的短詩";
        
        Console.WriteLine($"使用 {provider} 測試串流回應");
        Console.WriteLine($"測試訊息: {testMessage}");
        Console.WriteLine("串流回應:");
        Console.WriteLine("---");
        
        await foreach (var chunk in service.ProcessWithProviderStreamAsync(provider, testMessage))
        {
            Console.Write(chunk);
        }
        
        Console.WriteLine("\n---");
    }

    static async Task RunCustomMessageDemo(MultiProviderService service)
    {
        Console.WriteLine("=== 自訂訊息測試 ===");
        Console.Write("請輸入您的訊息: ");
        var userMessage = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            Console.WriteLine("訊息不能為空。");
            return;
        }

        var availableProviders = service.GetAvailableProviderTypes().ToList();
        if (!availableProviders.Any())
        {
            Console.WriteLine("沒有可用的 AI 提供者。");
            return;
        }

        Console.WriteLine("\n選擇處理方式:");
        Console.WriteLine("1. 選擇特定提供者");
        Console.WriteLine("2. 使用預設提供者");
        Console.WriteLine("3. 整合式處理 (多提供者)");
        Console.WriteLine("4. 串流回應");
        Console.Write("請選擇 (1-4): ");

        var method = Console.ReadLine();
        Console.WriteLine();

        switch (method)
        {
            case "1":
                Console.WriteLine("選擇提供者：");
                for (int i = 0; i < availableProviders.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableProviders[i]}");
                }
                Console.Write("請選擇: ");
                
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= availableProviders.Count)
                {
                    var selectedProvider = availableProviders[choice - 1];
                    Console.WriteLine($"\n使用 {selectedProvider} 處理中...");
                    var result = await service.ProcessWithProviderAsync(selectedProvider, userMessage);
                    Console.WriteLine($"回應: {result}");
                }
                else
                {
                    Console.WriteLine("無效選擇。");
                }
                break;
            case "2":
                Console.WriteLine("使用預設提供者處理中...");
                var defaultResult = await service.ProcessWithDefaultProviderAsync(userMessage);
                Console.WriteLine($"回應: {defaultResult}");
                break;
            case "3":
                Console.WriteLine("使用多提供者整合處理中...");
                var integratedResult = await service.ProcessIntegratedApproachAsync(userMessage);
                Console.WriteLine(integratedResult);
                break;
            case "4":
                Console.WriteLine("選擇提供者進行串流：");
                for (int i = 0; i < availableProviders.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {availableProviders[i]}");
                }
                Console.Write("請選擇: ");
                
                if (int.TryParse(Console.ReadLine(), out int streamChoice) && streamChoice > 0 && streamChoice <= availableProviders.Count)
                {
                    var streamProvider = availableProviders[streamChoice - 1];
                    Console.WriteLine($"\n使用 {streamProvider} 串流回應:");
                    Console.WriteLine("---");
                    await foreach (var chunk in service.ProcessWithProviderStreamAsync(streamProvider, userMessage))
                    {
                        Console.Write(chunk);
                    }
                    Console.WriteLine("\n---");
                }
                else
                {
                    Console.WriteLine("無效選擇。");
                }
                break;
            default:
                Console.WriteLine("無效選擇。");
                break;
        }
    }
}