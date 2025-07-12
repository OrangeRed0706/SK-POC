# 架構文檔

## 專案概覽

SK-POC 是一個展示 .NET Semantic Kernel 與 Anthropic Claude C# SDK 整合的概念驗證專案。

## 架構圖

```
┌─────────────────────────────────────────────────────────────┐
│                    SK-POC Application                      │
├─────────────────────────────────────────────────────────────┤
│                     Program.cs (Entry Point)               │
├─────────────────────────────────────────────────────────────┤
│                    Service Layer                           │
│  ┌─────────────────────┐  ┌─────────────────────────────────┐│
│  │ ClaudeSemanticKernel│  │   ClaudeChatCompletion         ││
│  │ Service             │  │   Service                      ││
│  │ (Main Orchestrator) │  │   (SK Integration)             ││
│  └─────────────────────┘  └─────────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│                     SDK Layer                              │
│  ┌─────────────────────┐  ┌─────────────────────────────────┐│
│  │ Microsoft.Semantic  │  │   Anthropic.SDK                ││
│  │ Kernel 1.60.0       │  │   5.4.3                        ││
│  └─────────────────────┘  └─────────────────────────────────┘│
├─────────────────────────────────────────────────────────────┤
│                     External APIs                          │
│  ┌─────────────────────────────────────────────────────────┐│
│  │           Anthropic Claude API                          ││
│  │         (claude-3-5-sonnet-20241022)                    ││
│  └─────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

## 核心元件

### 1. ClaudeSemanticKernelService
**職責**: 主要的服務協調器
- 管理 Semantic Kernel 實例
- 協調不同的處理模式
- 提供統一的 API 介面

**主要方法**:
- `ProcessWithSemanticKernelAsync()`: 使用 SK 處理
- `ProcessWithClaudeDirectAsync()`: 直接呼叫 Claude
- `ProcessWithIntegratedApproachAsync()`: 整合式處理
- `ProcessWithStreamingAsync()`: 串流回應

### 2. ClaudeChatCompletionService
**職責**: Semantic Kernel 整合層
- 實作 `IChatCompletionService` 介面
- 將 Claude API 適配為 SK 聊天完成服務
- 支援同步和串流模式

**核心功能**:
- 聊天歷史轉換
- 回應格式標準化
- 錯誤處理和重試

### 3. 設定管理
**職責**: 應用程式設定
- API Key 管理
- 模型設定
- 環境配置

## 整合模式

### 1. 直接整合
```csharp
// 直接使用 Anthropic.SDK
var client = new AnthropicClient(apiKey);
var response = await client.Messages.GetClaudeMessageAsync(parameters);
```

### 2. SK 插件模式
```csharp
// Claude 作為 SK 插件
var claudePlugin = kernel.CreatePluginFromType<ClaudePlugin>();
kernel.Plugins.Add(claudePlugin);
```

### 3. 自訂聊天完成服務
```csharp
// 註冊自訂服務
builder.Services.AddSingleton<IChatCompletionService>(
    sp => new ClaudeChatCompletionService(settings, client));
```

### 4. 整合式處理
```csharp
// 混合處理模式
var claudeResponse = await ProcessWithClaudeDirectAsync(message);
var skAnalysis = await ProcessWithSemanticKernelAsync(claudeResponse);
```

## 資料流

### 使用者輸入處理流程
```
使用者輸入 → 服務選擇 → API 呼叫 → 回應處理 → 輸出格式化
     ↓            ↓           ↓          ↓            ↓
   Console    Service     Claude API   TextContent   Console
   ReadLine   Router      /SK Kernel   Extraction    WriteLine
```

### 設定載入流程
```
appsettings.json → IConfiguration → AnthropicSettings → Service Registration
```

## 安全性考量

### API Key 管理
- 使用 .NET Configuration 系統
- 支援環境變數覆蓋
- .gitignore 排除敏感檔案

### 錯誤處理
- 結構化錯誤訊息
- 避免洩露敏感資訊
- 適當的重試機制

## 效能考量

### 記憶體管理
- 使用 `IAsyncEnumerable` 支援串流
- 適當的物件生命週期管理
- 避免記憶體洩漏

### 網路最佳化
- 連線池重用
- 適當的逾時設定
- 錯誤重試策略

## 擴展性

### 新增 AI 提供者
1. 實作 `IChatCompletionService`
2. 註冊到 DI 容器
3. 更新服務路由邏輯

### 新增功能
1. 擴展 `ClaudeSemanticKernelService`
2. 新增對應的示範範例
3. 更新文檔和測試

### 插件開發
1. 繼承 SK 插件基底類別
2. 使用 `[KernelFunction]` 屬性
3. 註冊到 Kernel 實例

## 測試策略

### 單元測試
- Service 類別的獨立測試
- Mock 外部依賴
- 測試不同的錯誤情境

### 整合測試
- 端到端 API 呼叫
- 真實環境測試
- 效能基準測試

### 手動測試
- 互動式 CLI 測試
- 不同輸入情境驗證
- 使用者體驗評估