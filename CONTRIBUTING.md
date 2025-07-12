# Contributing to SK-POC

感謝您對 SK-POC 專案的興趣！我們歡迎所有形式的貢獻。

## 如何貢獻

### 報告問題
- 使用 [GitHub Issues](https://github.com/OrangeRed0706/SK-POC/issues) 報告 bug 或提出功能請求
- 請提供詳細的重現步驟和環境資訊

### 提交程式碼
1. Fork 本專案
2. 建立功能分支：`git checkout -b feature/your-feature-name`
3. 進行更改並確保程式碼品質
4. 提交變更：`git commit -m "Add your feature"`
5. 推送到分支：`git push origin feature/your-feature-name`
6. 建立 Pull Request

## 開發環境設定

### 必要條件
- .NET 8.0 SDK
- Git
- Anthropic API Key

### 本地開發
```bash
git clone https://github.com/OrangeRed0706/SK-POC.git
cd SK-POC
cp src/SemanticKernel.Claude.POC/appsettings.example.json src/SemanticKernel.Claude.POC/appsettings.json
# 編輯 appsettings.json 填入您的 API Key
dotnet restore
dotnet build
dotnet run --project src/SemanticKernel.Claude.POC
```

## 程式碼風格

### .NET 慣例
- 使用 C# 慣用寫法
- 遵循 Microsoft 的 C# 編碼慣例
- 使用有意義的變數和方法名稱
- 適當的註解和文檔

### 專案結構
```
SK-POC/
├── src/                          # 原始碼
│   └── SemanticKernel.Claude.POC/ # 主要專案
├── docs/                         # 文檔
├── samples/                      # 範例程式碼
├── tests/                        # 測試專案
├── SK-POC.sln                   # 解決方案檔案
├── README.md                     # 專案說明
├── LICENSE                       # 授權條款
└── CONTRIBUTING.md              # 貢獻指南
```

## 提交指南

### Commit 訊息格式
```
<type>(<scope>): <subject>

<body>

<footer>
```

類型 (type)：
- `feat`: 新功能
- `fix`: 修復 bug
- `docs`: 文檔變更
- `style`: 程式碼格式化
- `refactor`: 重構
- `test`: 新增或修改測試
- `chore`: 建置過程或工具變更

### 範例
```
feat(integration): add streaming response support

Implement real-time streaming for Claude API responses
using async enumerable pattern.

Closes #123
```

## Pull Request 指南

### 檢查清單
- [ ] 程式碼可以成功建置
- [ ] 所有測試通過
- [ ] 符合程式碼風格指南
- [ ] 更新相關文檔
- [ ] 新增必要的測試
- [ ] 確保沒有洩露敏感資訊

### PR 描述模板
```markdown
## 變更描述
簡要描述此 PR 的變更內容

## 變更類型
- [ ] Bug 修復
- [ ] 新功能
- [ ] 重構
- [ ] 文檔更新
- [ ] 其他：___________

## 測試
描述如何測試這些變更

## 相關 Issue
Closes #(issue number)
```

## 授權
提交到此專案的所有貢獻都將採用 MIT 授權條款。

## 聯絡方式
如有任何疑問，請透過 GitHub Issues 與我們聯絡。