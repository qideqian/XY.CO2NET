using System;
using System.Collections.Generic;
using System.Text;

namespace XY.CO2NET
{
    /// <summary>
    /// XY.CO2NET 全局设置
    /// </summary>
    public class Config
    {
        // TODO:完善  在 startup.cs 中运行 RegisterServiceExtension.AddXYGlobalServices() 即可自动注入
 
        /// <summary>
        /// <para>全局配置</para>
        /// </summary>
        public static XYSetting XYSetting { get; set; } = new XYSetting();//TODO:需要考虑分布式的情况，后期需要储存在缓存中

        /// <summary>
        /// 指定是否是Debug状态，如果是，系统会自动输出日志
        /// </summary>
        public static bool IsDebug
        {
            get
            {
                return XYSetting.IsDebug;
            }
            set
            {
                XYSetting.IsDebug = value;
            }
        }

        /// <summary>
        /// 请求超时设置（以毫秒为单位），默认为10秒。
        /// 说明：此处常量专为提供给方法的参数的默认值，不是方法内所有请求的默认超时时间。
        /// </summary>
        public const int TIME_OUT = 10000;

        /// <summary>
        /// JavaScriptSerializer 类接受的 JSON 字符串的最大长度
        /// </summary>
        public static int MaxJsonLength = int.MaxValue;

        /// <summary>
        /// 默认缓存键的第一级命名空间，默认值：DefaultCache
        /// </summary>
        public static string DefaultCacheNamespace
        {
            get
            {
                return XYSetting.DefaultCacheNamespace ?? "DefaultCache";
            }
            set
            {
                XYSetting.DefaultCacheNamespace = value;
            }
        }

        private static string _rootDictionaryPath = null;

        /// <summary>
        /// 网站根目录绝对路径
        /// </summary>
        public static string RootDictionaryPath
        {
            get
            {
                if (_rootDictionaryPath == null)
                {
#if NET48
                    var appPath = AppDomain.CurrentDomain.BaseDirectory;
                    if (System.Text.RegularExpressions.Regex.Match(appPath, $@"[\\/]$", System.Text.RegularExpressions.RegexOptions.Compiled).Success)
                    {
                        _rootDictionaryPath = appPath;
                    }
#else
                    _rootDictionaryPath = AppContext.BaseDirectory;
#endif
                }
                return _rootDictionaryPath;
            }
            set
            {
                _rootDictionaryPath = value;
            }
        }
    }
}
