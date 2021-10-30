using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.RegisterServices
{
    /// <summary>
    /// 快捷注册接口
    /// </summary>
    public interface IRegisterService
    {

    }

    /// <summary>
    /// 快捷注册类，IRegisterService的默认实现
    /// </summary>
    public class RegisterService : IRegisterService
    {
        public static RegisterService Object { get; internal set; }

        private RegisterService() : this(null) { }

        private RegisterService(XYSetting xySetting)
        {
            //Senparc.CO2NET SDK 配置
            Config.XYSetting = xySetting ?? new XYSetting();
        }

#if !NET45
        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程（.NET Core）
        /// </summary>
        /// <param name="senparcSetting"></param>
        /// <returns></returns>
        public static RegisterService Start(XYSetting xySetting)
        {
            var register = new RegisterService(xySetting);

            //如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            register.RegisterThreads();//默认把线程注册好

            return register;
        }
#else
        /// <summary>
        /// 开始 Senparc.CO2NET SDK 初始化参数流程
        /// </summary>
        /// <returns></returns>
        public static RegisterService Start(XYSetting xySetting)
        {
            var register = new RegisterService(xySetting);

            //提供网站根目录
            Config.RootDictionaryPath = AppDomain.CurrentDomain.BaseDirectory;

            //如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
            register.RegisterThreads();//默认把线程注册好
            return register;
        }
#endif
    }
}
