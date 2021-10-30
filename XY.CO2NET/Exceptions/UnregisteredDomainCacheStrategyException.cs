using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Exceptions
{
    /// <summary>
    /// 领域缓存未注册异常
    /// </summary>
    public class UnregisteredDomainCacheStrategyException : CacheException
    {
        /// <summary>
        /// UnregisteredDomainCacheStrategyException 构造函数
        /// </summary>
        /// <param name="domainCacheStrategyType"></param>
        /// <param name="objectCacheStrategyType"></param>
        public UnregisteredDomainCacheStrategyException(Type domainCacheStrategyType, Type objectCacheStrategyType)
            : base($"当前扩展缓存策略没有进行注册：{domainCacheStrategyType}，{objectCacheStrategyType}，解决方案请参考：https://weixin.senparc.com/QA-551", null, true)
        {
            Trace.XYTrace.SendCustomLog("当前扩展缓存策略没有进行注册",
                $"当前扩展缓存策略没有进行注册，CacheStrategyDomain：{domainCacheStrategyType}，IBaseObjectCacheStrategy：{objectCacheStrategyType}");
        }
    }
}
