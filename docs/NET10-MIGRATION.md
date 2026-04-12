# XY.CO2NET 迁移说明（面向 .NET 10）

## 1. 目标框架与包版本策略

当前解决方案框架：

- `NET48`
- `netstandard2.0`
- `net6.0`
- `net10.0`

本次已统一为“按 TFM 分层”的包策略：

- **Legacy 层（`netstandard2.0`）**：`Microsoft.Extensions.* = 6.0.0`
- **Modern 层（`NET48` / `net6.0` / `net10.0`）**：`Microsoft.Extensions.* = 8.0.0`

说明：

- 这样可兼顾旧 TFM 的稳定性与新 TFM 的 API 兼容能力。
- 若后续准备完全对齐 .NET 10 生态，可将 Modern 层再提升到 `10.x`，建议分支验证后再合并。

## 2. 已进入“兼容模式”的 API（保留但不推荐）

以下 API 已加 `[Obsolete]`，用于提示迁移但不破坏现有调用：

1. `XY.Encrypt.MD5Encrypt.Encrypt(...)`
   - 原因：MD5 不适合安全敏感场景（存在碰撞风险）。
   - 推荐：`XY.Encrypt.SHA1Encrypt.SHA256(...)`（或业务侧更高等级方案，如 HMAC/SHA-256）。

2. `XY.Encrypt.DesEncrypt.Encrypt(...)`
3. `XY.Encrypt.DesEncrypt.Decrypt(...)`
   - 原因：DES 已过时，不适合安全敏感场景。
   - 推荐：`XY.CO2NET.Helpers.EncryptHelper` 中的 AES 相关方法（CBC），或业务侧进一步升级到更现代方案。

4. `XY.Encrypt.SHA1Encrypt.SHA1(...)`（含重载）
   - 原因：SHA1 不再推荐用于安全敏感场景。
   - 推荐：`XY.Encrypt.SHA1Encrypt.SHA256(...)`。

5. `XY.Encrypt.SHA1Encrypt.sha256(...)`（小写方法名）
   - 原因：命名不规范，已保留兼容。
   - 推荐：`XY.Encrypt.SHA1Encrypt.SHA256(...)`。

## 3. 关键行为修复（迁移收益）

- 修复 `EncryptHelper.GetCrc32(...)` 在 `toUpper=true` 时返回错误值的问题。
- 修复 `EncryptHelper.GetMD5(string, string)` 非法编码回退参数错误。
- 优化 `XYHttpClient.SetCookie(...)`，避免重复 Cookie 头导致异常。
- 若干加密实现补齐资源释放（`using`），降低运行时资源泄漏风险。

## 4. 对业务代码的迁移建议

建议按优先级替换：

1. 安全敏感链路：`DES/MD5/SHA1` -> `SHA256/AES`
2. 新代码禁止新增对 `[Obsolete]` API 的依赖
3. 对外契约（接口/协议）若短期不能改，可维持兼容 API，但内部逐步切换

## 5. net10.0 专项构建检查

建议在 CI 中加入以下检查：

```powershell
dotnet restore .\XY.CO2NET.sln
dotnet build .\XY.CO2NET\XY.CO2NET.csproj -c Release -f net10.0 --no-restore
dotnet build .\XY.CO2NET.Cache.Redis\XY.CO2NET.Cache.Redis.csproj -c Release -f net10.0 --no-restore
dotnet build .\XY.SMS\XY.SMS.csproj -c Release -f net10.0 --no-restore
```

本地执行通过后再发布，可降低跨 TFM 回归风险。
