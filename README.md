# Multi-AI Provider Semantic Kernel POC

這是一個展示如何整合多個 AI 提供者與 .NET Semantic Kernel 的概念驗證專案，支援 Claude、OpenAI、Azure OpenAI 和 Google Gemini。

## 功能特色

- **🤖 多 AI 提供者支援**: 
  - Claude (Anthropic.SDK 5.4.3)
  - OpenAI GPT-4o (Semantic Kernel)
  - Azure OpenAI (Semantic Kernel)
  - Google Gemini (Semantic Kernel Alpha)
- **🔄 智慧提供者管理**: 自動偵測可用提供者、優雅的錯誤處理
- **⚡ 多種處理模式**: 單一提供者測試、多提供者比較、協作式處理
- **📡 串流支援**: 所有提供者的實時回應
- **🎛️ 靈活配置**: 每個提供者的獨立設定和模型選擇
- **🔀 動態切換**: 執行時選擇和比較不同 AI 提供者

## 快速開始

### 1. 複製專案

```bash
git clone https://github.com/OrangeRed0706/SK-POC.git
cd SK-POC
```

### 2. 設定 API Key

複製範例設定檔並填入您的 AI 提供者 API Keys (只需設定您擁有的):

```bash
cp src/SemanticKernel.MultiProvider.POC/appsettings.example.json src/SemanticKernel.MultiProvider.POC/appsettings.json
```

編輯 `src/SemanticKernel.MultiProvider.POC/appsettings.json`:

```json
{
  "AISettings": {
    "DefaultProvider": "Claude",
    "Claude": {
      "ApiKey": "YOUR_ANTHROPIC_API_KEY_HERE",
      "Model": "claude-3-5-sonnet-20241022"
    },
    "OpenAI": {
      "ApiKey": "YOUR_OPENAI_API_KEY_HERE",
      "Model": "gpt-4o"
    },
    "AzureOpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/",
      "ApiKey": "YOUR_AZURE_OPENAI_API_KEY_HERE",
      "DeploymentName": "gpt-4o"
    },
    "Gemini": {
      "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
      "Model": "gemini-1.5-pro"
    }
  }
}
```

### 3. 安裝與執行

```bash
dotnet restore
dotnet build
dotnet run --project src/SemanticKernel.MultiProvider.POC
```

## 專案結構

```
SK-POC/
├── src/                                    # 原始碼目錄
│   └── SemanticKernel.MultiProvider.POC/  # 主專案
│       ├── SemanticKernel.MultiProvider.POC.csproj  # 專案檔案
│       ├── appsettings.example.json        # 設定檔範本
│       ├── Program.cs                      # 主程式入口
│       ├── Abstractions/                   # 抽象層
│       │   ├── IAIProvider.cs             # AI 提供者介面
│       │   ├── IAIProviderFactory.cs      # 提供者工廠介面
│       │   └── AIProviderBase.cs          # 提供者基底類別
│       ├── Configuration/                  # 設定模型
│       │   └── AISettings.cs             # 多提供者設定
│       ├── Providers/                      # AI 提供者實作
│       │   ├── ClaudeProvider.cs          # Claude 提供者
│       │   ├── OpenAIProvider.cs          # OpenAI 提供者
│       │   ├── AzureOpenAIProvider.cs     # Azure OpenAI 提供者
│       │   └── GeminiProvider.cs          # Gemini 提供者
│       ├── Services/                       # 服務層
│       │   ├── AIProviderFactory.cs       # 提供者工廠
│       │   └── MultiProviderService.cs    # 多提供者服務
│       └── Examples/                       # 整合範例
│           └── ClaudeIntegrationExamples.cs   # 範例程式
├── docs/                                   # 專案文檔
│   └── ARCHITECTURE.md                     # 架構文檔
├── samples/                                # 使用範例
│   └── BasicUsage.md                       # 基本使用說明
├── tests/                                  # 測試專案 (待新增)
├── SK-POC.sln                             # Visual Studio 解決方案檔案
├── LICENSE                                 # MIT 授權條款
├── CONTRIBUTING.md                         # 貢獻指南
├── .editorconfig                          # 編輯器設定
├── .gitignore                             # Git 忽略檔案
└── README.md                              # 專案說明
```

## 使用的套件版本

- **Microsoft.SemanticKernel**: 1.60.0 (最新穩定版)
- **Anthropic.SDK**: 5.4.3 (功能完整的非官方 SDK)
- **Microsoft.Extensions.***: 8.0.1+ (相依套件)

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
5. **自訂聊天完成服務**: 實作 IChatCompletionService 介面

## 安全性

- ✅ API Key 已被 `.gitignore` 排除，不會推送到版本控制
- ✅ 提供 `appsettings.example.json` 作為設定範本
- ✅ 所有敏感設定都從程式碼中分離

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
