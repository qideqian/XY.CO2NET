


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
