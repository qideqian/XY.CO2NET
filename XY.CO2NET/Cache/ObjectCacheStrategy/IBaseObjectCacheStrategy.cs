using System;
using System.Collections.Generic;
using System.Text;

namespace XY.CO2NET.Cache
{
    /// <summary>
    /// 所有以 string 类型为 key ，object 为 value 的缓存策略接口
    /// </summary>
    public interface IBaseObjectCacheStrategy : IBaseCacheStrategy<string, object>, IBaseCacheStrategy
    {
    }
}
