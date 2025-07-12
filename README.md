# .NET Semantic Kernel + Claude C# SDK POC

這是一個展示如何結合 .NET Semantic Kernel 和 Anthropic Claude C# SDK 的概念驗證專案。

## 功能特色

- **雙重整合**: 同時使用 Semantic Kernel 1.60.0 和 Anthropic.SDK 5.4.3
- **多種處理方式**: 支援直接 Claude API 呼叫、Semantic Kernel 處理、整合式處理
- **串流支援**: 實時顯示 Claude 回應
- **插件系統**: 將 Claude 封裝為 Semantic Kernel 插件
- **對話鏈**: 維護多輪對話上下文
- **函數呼叫**: 展示 Semantic Kernel 函數與 Claude 的結合

## 專案結構

```
POC0712/
├── SemanticKernel.Claude.POC.csproj  # 專案檔案
├── appsettings.json                   # 設定檔
├── Program.cs                         # 主程式入口
├── Models/
│   └── AnthropicSettings.cs          # Claude 設定模型
├── Services/
│   └── ClaudeSemanticKernelService.cs # 主要服務類別
└── Examples/
    └── ClaudeIntegrationExamples.cs   # 整合範例
```

## 安裝與設定

### 1. 安裝相依套件

```bash
dotnet restore
```

### 2. 設定 API Key

在 `appsettings.json` 中設定您的 Anthropic API Key:

```json
{
  "AnthropicSettings": {
    "ApiKey": "YOUR_ANTHROPIC_API_KEY_HERE",
    "Model": "claude-3-5-sonnet-20241022"
  }
}
```

### 3. 執行專案

```bash
dotnet run
```

## 使用的套件版本

- **Microsoft.SemanticKernel**: 1.60.0 (最新穩定版)
- **Anthropic.SDK**: 5.4.3 (功能完整的非官方 SDK)
- **Microsoft.Extensions.Configuration**: 8.0.0
- **Microsoft.Extensions.DependencyInjection**: 8.0.0
- **Microsoft.Extensions.Logging**: 8.0.0

## 核心功能展示

### 1. 基本 Claude API 呼叫
直接使用 Anthropic.SDK 與 Claude API 互動。

### 2. Semantic Kernel 基本功能
使用 Semantic Kernel 的提示工程和函數呼叫功能。

### 3. 整合式處理
結合 Claude 和 Semantic Kernel，先用 Claude 處理，再用 SK 分析結果。

### 4. 串流回應
實時顯示 Claude 的回應內容。

### 5. 對話鏈範例
維護多輪對話的上下文。

### 6. 函數呼叫範例
展示 Semantic Kernel 函數與 Claude 的協作。

### 7. Claude 插件
將 Claude 封裝為 Semantic Kernel 插件供其他功能呼叫。

## 技術架構

本專案展示了以下整合模式：

1. **直接整合**: 直接使用兩個 SDK 的 API
2. **插件模式**: 將 Claude 包裝成 Semantic Kernel 插件
3. **服務層整合**: 透過服務層統一管理兩個 SDK
4. **配置管理**: 使用 .NET 的配置系統管理 API 金鑰和設定

## 注意事項

- 需要有效的 Anthropic API Key
- 支援 .NET 8.0
- Claude 3.5 Sonnet 模型為預設選擇
- 所有 API 呼叫都包含錯誤處理

## 擴展建議

- 新增更多 Semantic Kernel 插件
- 實作自訂的對話記憶體管理
- 整合其他 AI 服務
- 新增單元測試和整合測試