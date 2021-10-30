using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Cache
{
    /// <summary>
    /// 缓存序列化扩展方法，所有（分布式）缓存的序列化、反序列化过程必须使用这里的方法统一读写
    /// </summary>
    public static class CacheSerializeExtension
    {
        /// <summary>
        /// 序列化到缓存可用的对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeToCache<T>(this T obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return json;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object DeserializeFromCache(this string value, Type type = null)
        {
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(value, type);
            return obj;
        }

        /// <summary>
        /// 从缓存对象反序列化到实例（效率更高，推荐）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeFromCache<T>(this string value)
        {
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
            return obj;
        }
    }
}
