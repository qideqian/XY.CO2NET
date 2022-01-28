using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XY.SMS.SmsPlatform;

namespace XY.SMS
{
    /// <summary>
    /// 短信注册
    /// </summary>
    public static partial class Register
    {
        /// <summary>
        /// 将短信配置数据注入到DI
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSMSServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<XYSmsSetting>(configuration.GetSection("XYSmsSetting"));
        }
    }
}
