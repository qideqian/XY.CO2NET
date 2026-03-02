#if !NET48
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XY.CO2NET
{
    /// <summary>
    /// 针对 .NET Core 的依赖注入扩展类
    /// </summary>
    public static class XYDI
    {
        public const string XY_DI_THREAD_SERVICE_SCOPE = "___XYDIThreadScope";

        /// <summary>
        /// 全局 ServiceCollection
        /// </summary>
        public static IServiceCollection GlobalServiceCollection { get; set; }

        /// <summary>
        /// 从 GlobalServiceCollection 重新 Build，生成新的 IServiceProvider
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider()
        {
            return GlobalServiceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// 线程内的 单一 Scope 范围 ServiceScope
        /// </summary>
        public static IServiceScope ThreadServiceScope
        {
            get
            {
                var threadServiceScope = Thread.GetData(Thread.GetNamedDataSlot(XY_DI_THREAD_SERVICE_SCOPE)) as IServiceScope;
                return threadServiceScope;
            }
        }

        /// <summary>
        /// 通过 GetServiceProvider() 方法执行 .GetService() 方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetService(Type type)
        {
            return GetServiceProvider().GetService(type);
        }
    }
}
#endif
