# Multi-AI Provider MCP Integration POC

這是一個展示如何整合多個 AI 提供者與 Model Context Protocol (MCP) 的概念驗證專案，讓使用者可以透過 API Key 選擇 AI 提供者並呼叫 MCP 功能。

## 功能特色

- **🤖 多 AI 提供者支援**: 
  - Claude (Anthropic.SDK 5.4.3)
  - OpenAI GPT-4o (Semantic Kernel)
  - Azure OpenAI (Semantic Kernel)
  - Google Gemini (Semantic Kernel Alpha)
- **🔧 MCP 工具整合**: 支援各種 MCP 工具呼叫
  - 檔案系統操作 (讀取/寫入)
  - 網路搜尋
  - 計算功能
  - 資料庫查詢
- **🤝 AI + MCP 協作**: AI 提供者與 MCP 工具的智慧整合
- **⚙️ 靈活配置**: 每個提供者的獨立設定和模型選擇
- **🔀 動態選擇**: 執行時選擇 AI 提供者和 MCP 工具

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
- **ModelContextProtocol**: 0.3.0-preview.2 (MCP 整合)
- **Microsoft.Extensions.AI**: 9.6.0 (AI 抽象層)
- **Microsoft.Extensions.***: 8.0.1+ (相依套件)

## 核心功能展示

### 1. AI 提供者選擇與測試
選擇不同的 AI 提供者進行對話測試。

### 2. MCP 工具呼叫
直接呼叫各種 MCP 工具，如檔案操作、網路搜尋等。

### 3. AI + MCP 整合協作
AI 提供者分析使用者需求，自動選擇合適的 MCP 工具，並整合結果。

### 4. 互動式選單
直觀的命令列介面，支援：
- 提供者測試
- MCP 工具呼叫
- 整合式協作示範

## 技術架構

本專案展示了以下整合模式：

1. **多提供者抽象**: 透過 IAIProvider 介面統一不同 AI 服務
2. **MCP 工具整合**: 使用 Model Context Protocol 進行工具呼叫
3. **AI + MCP 協作**: AI 提供者與 MCP 工具的智慧協作
4. **配置管理**: 使用 .NET 的配置系統管理 API 金鑰和設定
5. **依賴注入**: 使用 Microsoft.Extensions.DependencyInjection

## 安全性

- ✅ API Key 已被 `.gitignore` 排除，不會推送到版本控制
- ✅ 提供 `appsettings.example.json` 作為設定範本
- ✅ 所有敏感設定都從程式碼中分離

## 注意事項

- 支援 .NET 8.0
- 需要至少一個有效的 AI 提供者 API Key
- 目前 MCP 工具為模擬實作，實際使用需連接真實 MCP 伺服器
- 所有 API 呼叫都包含錯誤處理

## 擴展建議

- 整合真實的 MCP 伺服器
- 新增更多 MCP 工具支援
- 實作自訂的對話記憶體管理
- 新增單元測試和整合測試
- 支援更多 AI 提供者
