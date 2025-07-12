# 基本使用範例

本文檔提供 SK-POC 專案的基本使用範例。

## 快速開始

### 1. 環境設定

```bash
# 複製專案
git clone https://github.com/OrangeRed0706/SK-POC.git
cd SK-POC

# 設定 API Key
cp src/SemanticKernel.Claude.POC/appsettings.example.json src/SemanticKernel.Claude.POC/appsettings.json
# 編輯 appsettings.json 填入您的 Anthropic API Key

# 建置與執行
dotnet restore
dotnet build
dotnet run --project src/SemanticKernel.Claude.POC
```

### 2. 互動式示範

執行程式後，您會看到以下選單：

```
=== .NET Semantic Kernel + Claude C# SDK POC ===

選擇示範類型:
1. 基本 Claude API 呼叫
2. Semantic Kernel 基本功能
3. 整合式處理 (Claude + SK)
4. 串流回應
5. 對話鏈範例
6. 函數呼叫範例
7. 自訂訊息測試
0. 離開

請選擇 (0-7):
```

## 程式碼範例

### 基本 Claude API 呼叫

```csharp
var client = new AnthropicClient("your-api-key");
var messages = new List<Message>
{
    new Message(RoleType.User, "解釋什麼是機器學習")
};

var parameters = new MessageParameters
{
    Messages = messages,
    Model = "claude-3-5-sonnet-20241022",
    MaxTokens = 1000,
    Stream = false
};

var response = await client.Messages.GetClaudeMessageAsync(parameters);
var textContent = response.Content.OfType<TextContent>().FirstOrDefault();
Console.WriteLine($"Claude 回應: {textContent?.Text}");
```

### Semantic Kernel 整合

```csharp
// 建立 Kernel 實例
var builder = Kernel.CreateBuilder();
builder.Services.AddSingleton<IChatCompletionService>(
    sp => new ClaudeChatCompletionService(settings, client));
var kernel = builder.Build();

// 使用 SK 處理提示
var function = kernel.CreateFunctionFromPrompt(@"
    分析以下文字並提供摘要：{{$input}}
");

var result = await kernel.InvokeAsync(function, new() { 
    ["input"] = "您的輸入文字" 
});
Console.WriteLine($"SK 結果: {result}");
```

### 串流回應

```csharp
var parameters = new MessageParameters
{
    Messages = messages,
    Model = "claude-3-5-sonnet-20241022",
    MaxTokens = 1000,
    Stream = true
};

await foreach (var result in client.Messages.StreamClaudeMessageAsync(parameters))
{
    if (result.Delta?.Text != null)
    {
        Console.Write(result.Delta.Text);
    }
}
```

### Claude 插件使用

```csharp
public class ClaudePlugin
{
    private readonly AnthropicClient _client;

    public ClaudePlugin(AnthropicClient client)
    {
        _client = client;
    }

    [KernelFunction("ask_claude")]
    [Description("向 Claude AI 詢問問題")]
    public async Task<string> AskClaudeAsync(
        [Description("要詢問的問題")] string question)
    {
        var messages = new List<Message>
        {
            new Message(RoleType.User, question)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            Model = "claude-3-5-sonnet-20241022",
            MaxTokens = 500
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters);
        var textContent = response.Content.OfType<TextContent>().FirstOrDefault();
        return textContent?.Text ?? "無法取得回應";
    }
}

// 註冊和使用插件
var plugin = kernel.CreatePluginFromType<ClaudePlugin>();
kernel.Plugins.Add(plugin);
```

### 整合式處理範例

```csharp
public async Task<string> ProcessWithIntegratedApproachAsync(string userMessage)
{
    // 步驟 1: 使用 Claude 直接處理
    var claudeResponse = await ProcessWithClaudeDirectAsync(userMessage);
    
    // 步驟 2: 使用 SK 分析 Claude 的回應
    var analysisPrompt = $@"
        分析以下 AI 回應並提供：
        1. 重點摘要
        2. 額外見解
        3. 後續問題建議
        
        原始問題：{userMessage}
        AI 回應：{claudeResponse}
    ";
    
    var skResponse = await ProcessWithSemanticKernelAsync(analysisPrompt);
    
    return $@"
=== Claude 直接回應 ===
{claudeResponse}

=== Semantic Kernel 分析 ===
{skResponse}
    ";
}
```

## 常見使用情境

### 1. 問答系統
```csharp
var question = "什麼是深度學習？";
var response = await service.ProcessWithClaudeDirectAsync(question);
```

### 2. 文本分析
```csharp
var analysisPrompt = "分析以下文本的情感和主題：" + inputText;
var analysis = await service.ProcessWithSemanticKernelAsync(analysisPrompt);
```

### 3. 多輪對話
```csharp
var conversation = new List<Message>();
conversation.Add(new Message(RoleType.User, "你好"));
// ... 持續新增對話
```

### 4. 函數呼叫
```csharp
var weatherFunction = kernel.CreateFunctionFromMethod(
    (string location) => $"The weather in {location} is sunny",
    "GetWeather"
);
var result = await kernel.InvokeAsync(weatherFunction, 
    new() { ["location"] = "Taipei" });
```

## 最佳實踐

### 1. API Key 安全
- 永不將 API Key 硬編碼在程式中
- 使用環境變數或設定檔
- 確保 `.gitignore` 包含設定檔

### 2. 錯誤處理
```csharp
try
{
    var response = await service.ProcessWithClaudeDirectAsync(message);
    return response;
}
catch (HttpRequestException ex)
{
    logger.LogError(ex, "API 呼叫失敗");
    return "抱歉，目前無法處理您的請求";
}
```

### 3. 資源管理
```csharp
using var scope = serviceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IClaudeService>();
```

### 4. 效能最佳化
- 使用串流模式處理長回應
- 實作適當的快取機制
- 考慮使用連線池

## 疑難排解

### 常見問題

1. **API Key 錯誤**
   - 確認 API Key 格式正確
   - 檢查 appsettings.json 設定

2. **網路連線問題**
   - 檢查網路連線
   - 確認防火牆設定

3. **回應格式錯誤**
   - 確認使用正確的 Content 類型
   - 檢查 API 版本相容性

### 除錯技巧

1. 啟用詳細記錄
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

2. 使用中斷點調試
3. 檢查 API 回應原始內容