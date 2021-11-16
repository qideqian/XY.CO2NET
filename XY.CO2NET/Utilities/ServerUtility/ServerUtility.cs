using System;
using System.IO;
#if NET45
using System.Web;
#endif

namespace XY.CO2NET.Utilities
{
    /// <summary>
    /// 服务器工具类
    /// </summary>
    public class ServerUtility
    {
        private static string _appDomainAppPath;

        /// <summary>
        /// dll 项目根目录
        /// </summary>
        public static string AppDomainAppPath
        {
            get
            {
                if (_appDomainAppPath == null)
                {
#if NET45
                    _appDomainAppPath = HttpRuntime.AppDomainAppPath;
#else
                    _appDomainAppPath = AppContext.BaseDirectory; //dll所在目录：;
#endif
                }
                return _appDomainAppPath;
            }
            set
            {
                _appDomainAppPath = value;
#if !NET45
                var pathSeparator = Path.DirectorySeparatorChar.ToString();
                var altPathSeparator = Path.AltDirectorySeparatorChar.ToString();
                if (!_appDomainAppPath.EndsWith(pathSeparator) && !_appDomainAppPath.EndsWith(altPathSeparator))
                {
                    _appDomainAppPath += pathSeparator;
                }
#endif
            }
        }

        /// <summary>
        /// 获取相对于网站根目录的文件路径
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static string ContentRootMapPath(string virtualPath)
        {
            if (virtualPath == null)
            {
                return "";
            }
            else
            {
                //if (!Config.RootDictionaryPath.EndsWith("/") || Config.RootDictionaryPath.EndsWith("\\"))
                var pathSeparator = Path.DirectorySeparatorChar.ToString();
                var altPathSeparator = Path.AltDirectorySeparatorChar.ToString();
                if (!Config.RootDictionaryPath.EndsWith(pathSeparator) && !Config.RootDictionaryPath.EndsWith(altPathSeparator))
                {
                    Config.RootDictionaryPath += pathSeparator;
                }

                if (virtualPath.StartsWith("~/"))
                {
                    return virtualPath.Replace("~/", Config.RootDictionaryPath).Replace("/", pathSeparator);
                }
                else
                {
                    return Path.Combine(Config.RootDictionaryPath, virtualPath);
                }
            }
        }

        /// <summary>
        /// 获取相对于dll目录的文件绝对路径
        /// </summary>
        /// <param name="virtualPath">虚拟路径，如~/App_Data/</param>
        /// <returns></returns>
        public static string DllMapPath(string virtualPath)
        {
            if (virtualPath == null)
            {
                return "";
            }
            else if (virtualPath.StartsWith("~/"))
            {
                var pathSeparator = Path.DirectorySeparatorChar.ToString();
                return virtualPath.Replace("~/", AppDomainAppPath).Replace("/", pathSeparator);
            }
            else
            {
                return Path.Combine(AppDomainAppPath, virtualPath);
            }
        }

    }
}
