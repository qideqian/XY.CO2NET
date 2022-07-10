using System;
using System.Collections.Generic;
using System.Text;

namespace XY.CO2NET
{
    /// <summary>
    /// CO2NET 全局设置
    /// </summary>
    public class XYSetting
    {
        /// <summary>
        /// XYSetting 构造函数
        /// </summary>
        public XYSetting() : this(false)
        {
        }

        /// <summary>
        /// XYSetting 构造函数
        /// </summary>
        public XYSetting(bool isDebug)
        {
            IsDebug = isDebug;
        }

        /// <summary>
        /// 是否出于Debug状态
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// 默认缓存键的第一级命名空间，默认值：DefaultCache
        /// </summary>
        public string DefaultCacheNamespace { get; set; }

        /// <summary>
        /// XY 统一代理标识
        /// </summary>
        public string XYUnionAgentKey { get; set; }

        #region 分布式缓存

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string Cache_Redis_Configuration { get; set; }

        /// <summary>
        /// Memcached连接字符串
        /// </summary>
        public string Cache_Memcached_Configuration { get; set; }

        #endregion

#if NET48
        /// <summary>
        /// 从 Web.Config 文件自动生成 XYSetting
        /// </summary>
        /// <param name="isDebug">设置 CO2NET 全局的 Debug 状态 </param>
        /// <returns></returns>
        public static XYSetting BuildFromWebConfig(bool isDebug)
        {
            var xySetting = new XYSetting(isDebug);

            xySetting.DefaultCacheNamespace = System.Configuration.ConfigurationManager.AppSettings["DefaultCacheNamespace"];
            xySetting.XYUnionAgentKey = System.Configuration.ConfigurationManager.AppSettings["XYUnionAgentKey"];
            xySetting.Cache_Redis_Configuration = System.Configuration.ConfigurationManager.AppSettings["Cache_Redis_Configuration"];
            xySetting.Cache_Memcached_Configuration = System.Configuration.ConfigurationManager.AppSettings["Cache_Memcached_Configuration"];
            return xySetting;
        }
#endif
    }
}
