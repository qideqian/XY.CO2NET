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
        /// 将短信配置数据注入到DI（原有方法保留）
        /// </summary>
        public static IServiceCollection AddSMSServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<XYSmsSetting>(configuration.GetSection("XYSmsSetting"));
            services.AddSingleton<ISmsPlatform>(service => SmsPlatformFactory.GetSmsPlatform(service));
            return services;
        }

        /// <summary>
        /// 从配置文件 section "XYSmsSetting" 绑定并注册 XYSmsSetting 到 DI 容器
        /// </summary>
        public static IServiceCollection AddSmsSetting(this IServiceCollection services, IConfiguration configuration)
        {
            var smsSetting = new SmsPlatform.XYSmsSetting();
            configuration.GetSection("XYSmsSetting").Bind(smsSetting);
            services.AddSingleton(smsSetting);
            return services;
        }
    }
}
