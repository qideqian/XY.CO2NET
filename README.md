# XY.CO2NET

[![GitHub stars](https://img.shields.io/github/stars/qideqian/XY.CO2NET.svg?style=social)](https://github.com/qideqian/XY.CO2NET/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/qideqian/XY.CO2NET.svg?style=social)](https://github.com/qideqian/XY.CO2NET/network/members)
[![GitHub issues](https://img.shields.io/github/issues/qideqian/XY.CO2NET.svg)](https://github.com/qideqian/XY.CO2NET/issues)
[![GitHub license](https://img.shields.io/github/license/qideqian/XY.CO2NET.svg)](https://github.com/qideqian/XY.CO2NET/blob/master/LICENSE)

## 项目说明

XY.CO2NET 是一个多项目基础能力集合仓库，覆盖缓存、短信、OCR、AI 结构化提取与 ASP.NET Core MVC 扩展能力。

当前主要类库包括：

- `XY.CO2NET`：通用基础类库（缓存、加密、HTTP、线程等）
- `XY.CO2NET.Cache.Redis`：Redis 缓存扩展
- `XY.SMS`：短信服务封装（阿里云）
- `XY.OCR.AlibabaCloud`：阿里云 OCR 服务封装
- `XY.OCR.UmiOCR`：UmiOCR 服务封装
- `XY.AI.SemanticKernel`：基于 Semantic Kernel 的 AI 结构化数据提取
- `XY.AspNetCore.Mvc.Validation`：ASP.NET Core MVC 参数验证扩展

## 目标框架

仓库内项目按功能分别支持以下框架组合：

- `NET48`
- `netstandard2.0`
- `net6.0`
- `net10.0`

## NuGet 包发布说明（更新）

- 多项目已统一补充 `PackageIcon`、`PackageReadmeFile`、License 与仓库元数据
- 包图标统一复用 `XY.OCR.AlibabaCloud/icon.png`
- 部分项目启用了 Central Package Management（`Directory.Packages.props`）

如遇 CI 还原失败（NU1100）请检查仓库根目录 `NuGet.Config` 的 `packageSourceMapping` 是否覆盖所用包前缀。

## master 自动发布 NuGet

提交推送到 GitHub 的 `master` 后，`.github/workflows/nuget-publish.yml` 自动完成：

1. 读取当前发布线的 Git 标签和 NuGet 已发布版本，递增补丁号。
2. 使用 `global.json` 指定的 .NET SDK，在 Windows 上构建并运行全部测试。
3. 使用同一版本打包 7 个类库，校验包版本、目标框架程序集、内部依赖和 OCR 符号包。
4. 保存包到本次 Actions 的 Artifacts，通过 NuGet 可信发布取得临时凭据，创建指向本次提交的 `v版本号` 注释标签。
5. 向 nuget.org 上传全部包和符号包，并在 Actions 摘要中记录版本与提交。

日常提交后执行 `git push origin master` 即可，无需手动修改各项目的 `<Version>` 或创建标签。自动版本只在构建时传入，不回写源码，因此不会产生机器人提交循环。推送标签本身不会触发此工作流。

### 版本规则

发布配置集中在 `.github/nuget-release.json`：

| 字段 | 含义 |
| --- | --- |
| `versionPrefix` | 当前主次版本线，现为 `2.0`，只在这一版本线内递增补丁号 |
| `minimumVersion` | 新发布允许使用的最低版本，现为 `2.0.10` |
| `projects` | 待发布项目清单，项目文件名即包名；核心包排在依赖它的包之前 |

自动发布的版本下限为 `2.0.10`，之后通常为 `2.0.11`、`2.0.12`。如果标签或 NuGet 已经占用了更高的 `2.0.x` 版本，则跳到其后的补丁号。仅正式三段式版本参与计算，历史 `v20.0.4` 和预发布版本不影响这条发布线。标签创建后上传失败也会保留版本占用，不会把该标签移到另一个提交。

注意：部分包曾发布过 `20.0.4`。继续发布 `2.0.x` 不会让使用 `20.0.4` 的消费者自动降级；这类项目需要显式选择 `2.0.x` 版本。

多个推送共用发布队列，依次分配版本，最多支持 100 个等待中的任务。以后升级主次版本时，同时修改 `versionPrefix` 和 `minimumVersion`。

### GitHub 配置和失败重试

- 使用 NuGet 的 [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) 获取短期凭据，不再读取仓库中旧的 `NUGET_API_KEY` Secret，也无需定期手工更新长期密钥。
- NuGet 账户 `qideqian` 的可信发布策略需对应仓库所有者 `qideqian`、仓库 `XY.CO2NET`、工作流文件名 `nuget-publish.yml`、环境 `production`，并允许发布上述包的新版本。工作流通过 `NuGet/login@v1` 和 `id-token: write` 使用这条已有策略；账户名是公开名称，不是密钥。
- 工作流使用自带的 `GITHUB_TOKEN`，通过 `contents: write` 创建标签。如果仓库标签规则限制创建 `v*`，需要允许该工作流；标签推送失败时不会开始上传 NuGet。
- 构建、测试、包检查或临时凭据获取失败时，不创建标签、不上传包。标签表示该版本已绑定到指定提交，最终是否发布成功以 Actions 结果为准。
- 标签创建成功后若上传中断，在原 Actions 运行中选择 **Re-run failed jobs**。脚本会复用该提交已有的标签，跳过已经发布的包并补发剩余包和符号包，不额外增加版本，也不移动标签。
- 支持在 Actions 中选择 `master` 手动运行；其他分支会跳过发布。同一个已打标签的提交再次手动运行，也会复用原版本。

### 本地验证

在仓库根目录使用 PowerShell 7 执行：

```powershell
# 只使用临时仓库和模拟 HTTP 响应，验证版本选择及异常处理。
pwsh -NoProfile -File .github/scripts/Test-NuGetReleaseVersion.ps1

# 只向本地临时裸仓库推送，验证标签创建、重跑和冲突保护。
pwsh -NoProfile -File .github/scripts/Test-NuGetReleaseTag.ps1

# 只读查询真实 Git 标签和 NuGet 版本，不创建标签、不上传包。
$release = ./.github/scripts/Get-NuGetReleaseVersion.ps1

# 还原、构建、测试、打包和检查；输出目录必须为空。
./.github/scripts/Invoke-NuGetReleaseBuild.ps1 -Version $release.Version -OutputDirectory artifacts/nuget-local
```

`artifacts/` 已被 Git 忽略。本地验证不会向 GitHub 或 NuGet 写入数据；真正发布需将配置和脚本一并提交并推送到 `master`。

## 安全扫描（gitleaks）

仓库根目录已提供 `.gitleaks.toml` 配置文件。

- 本地扫描：

```pwsh
gitleaks detect --source . --config .gitleaks.toml --redact --verbose
```

- CI 扫描（发现泄露时返回非 0）：

```bash
gitleaks detect --source . --config .gitleaks.toml --redact --exit-code 1
```
