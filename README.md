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
