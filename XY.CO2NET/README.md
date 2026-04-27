



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

> ### _本地缓存使用_
#### 缓存注册（不注册默认使用本地缓存）
```
//注册Redis缓存
XY.CO2NET.Cache.Redis.Register.SetConfigurationOption('Cache_Redis_Configuration');
XY.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow();
```
##### 基本使用
```
var cache = CacheStrategyFactory.GetObjectCacheStrategyInstance();//获取缓存策略
cache.Set("key", Value);//设定值
cache.Get<T>("key");//获取值
```
方法缓存参考接口：`public static T GetMethodCache<T>(string cacheKey, Func<T> func, int timeoutSeconds) where T : class`
***

> 缓存锁的使用
```
using (await Cache.BeginCacheLockAsync("Key", "").ConfigureAwait(false))
{
}
```
***
> XYMessageQueue消息队列使用

启动线程消费队列`XYMessageQueue.OperateQueue();`

```
XYMessageQueue messageQueue = new XYMessageQueue();
messageQueue.Add("key", () => action());
```
***
> 日志使用
```
XY.CO2NET.Trace.XYTrace.Log("MSG");
XY.CO2NET.Trace.XYTrace.XXXLog("MSG");
```
***
> 线程使用

可通过将需要运行的后台线程方法添加到静态属性`ThreadUtility.AsynThreadCollection`然后通过`ThreadUtility.Register();`启动

> 各项初始化配置

NET48模式下通过`RegisterService.Start(XYSetting xySetting)`注册

NET Core模式下通过`RegisterService.Start(XYSetting xySetting)`注册


> Http处理

1.HttpClient
2.Get
3.Post



> 配置文件的注册


***

# .NetCore下各项使用

+ 缓存在表数据查询中的应用

+ .NetCore下缓存应用，分布式缓存应用


推送标签并出发Nuget发布
```
cd 'D:\Project\GitHub\XY.CO2NET' 
git tag -a v2.0.7 -m "Release v2.0.7" 
git push origin refs/tags/v2.0.7
```

删除本地标签
```
git tag -d v2.0.0
```

删除远程标签
```
git push origin --delete v2.0.0
```
