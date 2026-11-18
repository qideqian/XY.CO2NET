using System;
using System.Collections.Generic;
using System.Linq;
using XY.CO2NET.Helpers;
using XY.CO2NET.Trace;

namespace XY.CO2NET.Cache
{
    /// <summary>
    /// 领域缓存仓库
    /// </summary>
    public class CacheStrategyDomainWarehouse
    {
        private static Dictionary<string, CacheStrategyDomainMappingCollection> _extensionCacheStrategyInstance
            = new Dictionary<string, CacheStrategyDomainMappingCollection>();

        /// <summary>
        /// 获取某个领域内的所有CacheStrategyDomainMappingCollection
        /// </summary>
        /// <param name="identityName"></param>
        /// <returns></returns>
        private static CacheStrategyDomainMappingCollection GetMappingCollection(string identityName)
        {
            if (!_extensionCacheStrategyInstance.ContainsKey(identityName))
            {
                _extensionCacheStrategyInstance[identityName] = new CacheStrategyDomainMappingCollection();
            }

            return _extensionCacheStrategyInstance[identityName];
        }

        static CacheStrategyDomainWarehouse()
        {

        }

        /// <summary>
        /// 注册领域缓存
        /// </summary>
        /// <param name="domainCacheStrategy"></param>
        public static void RegisterCacheStrategyDomain(IDomainExtensionCacheStrategy domainCacheStrategy)
        {
            var identityName = domainCacheStrategy.CacheStrategyDomain.IdentityName;
            var cacheStrategy = domainCacheStrategy.BaseCacheStrategy();
            var mappingCollection = GetMappingCollection(identityName);
            var mappingItem = new CacheStrategyDomainMappingItem(domainCacheStrategy);
            mappingCollection[cacheStrategy] = mappingItem;
        }

        /// <summary>
        /// 获取指定领域与基础缓存策略之间已登记的扩展缓存策略。
        /// </summary>
        /// <param name="baseObjectCacheStrategy">IBaseObjectCacheStrategy 缓存策略对象</param>
        /// <param name="cacheStrategyDomain">缓存领域</param>
        /// <returns>已登记的领域扩展缓存策略实例。</returns>
        /// <exception cref="Exceptions.UnregisteredDomainCacheStrategyException">尚未登记该领域与基础缓存策略的映射。</exception>
        public static IDomainExtensionCacheStrategy GetDomainExtensionCacheStrategy(IBaseObjectCacheStrategy baseObjectCacheStrategy,
            ICacheStrategyDomain cacheStrategyDomain)
        {
            var identityName = cacheStrategyDomain.IdentityName;
            var mappingCollection = GetMappingCollection(identityName);//当前扩展缓存可能已经注册的所有基础缓存

            if (mappingCollection.ContainsKey(baseObjectCacheStrategy))
            {
                var item = mappingCollection[baseObjectCacheStrategy];
                return item.DomainExtensionCacheStrategy;
            }
            else
            {
                //没有对应登记时调用日志记录并抛出未注册异常；此处不回退到本地缓存。
                var ex = new Exceptions.UnregisteredDomainCacheStrategyException(cacheStrategyDomain.GetType(), baseObjectCacheStrategy.GetType());
                XYTrace.BaseExceptionLog(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 清空所有已经注册的领域缓存对象
        /// </summary>
        public static void ClearRegisteredDomainExtensionCacheStrategies()
        {
            _extensionCacheStrategyInstance.Clear();
        }

        /// <summary>
        /// 遍历手动提供的扩展策略，并可选反射访问已加载程序集中的扩展策略。
        /// </summary>
        /// <remarks>本方法不直接登记缓存映射；登记依赖实例提供器或策略初始化自身的副作用。</remarks>
        /// <param name="autoScanExtensionCacheStrategies">是否扫描当前已加载程序集中的非抽象扩展策略类型，并尝试读取其 Instance 属性；扫描会增加启动开销。</param>
        /// <param name="extensionCacheStrategiesFunc"><para>可选的手动策略实例提供器。</para>
        /// <para>委托为 null 或返回 null 时跳过手动列表；是否反射扫描只由 autoScanExtensionCacheStrategies 控制。</para></param>
        /// <returns>本次收集的手动实例类型和扫描类型，可能重复；不保证 Instance 获取成功或策略已经登记。</returns>
        public static List<Type> AutoScanDomainCacheStrategy(bool autoScanExtensionCacheStrategies = false, Func<IList<IDomainExtensionCacheStrategy>> extensionCacheStrategiesFunc = null)
        {
            //遍历候选扩展策略，并按开关尝试反射访问。
            var dt1 = SystemTime.Now;
            var addedTypes = new List<Type>();
            var cacheTypes = "";//本次收集的扩展策略类型

            if (extensionCacheStrategiesFunc != null)
            {
                var containerCacheStrategies = extensionCacheStrategiesFunc();
                if (containerCacheStrategies != null)
                {
                    foreach (var cacheStrategy in containerCacheStrategies)
                    {
                        var exCache = cacheStrategy;//仅取得已提供的实例，不在此处执行登记。

                        var cacheType = exCache.GetType();
                        cacheTypes += "\r\n" + cacheType;
                        addedTypes.Add(cacheType);
                    }
                }
            }

            var scanTypesCount = 0;
            if (autoScanExtensionCacheStrategies)
            {
                //查找所有扩展缓存  TODO:扫描程序可以集中到一个 Helper 或者 Utility 中
                var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a =>
                            {
                                try
                                {
                                    scanTypesCount++;
                                    var aTypes = a.GetTypes();
                                    return aTypes.Where(t => !t.IsAbstract &&/* !officialTypes.Contains(t) &&*/ t.GetInterfaces().Contains(typeof(IDomainExtensionCacheStrategy)));
                                }
                                catch (Exception ex)
                                {
                                    XYTrace.SendCustomLog("UseXYGlobal() 自动扫描程序集异常：" + a.FullName, ex.ToString());
                                    return new List<Type>();//不能 return null
                                }
                            });

                if (types != null)
                {
                    foreach (var type in types)
                    {
                        if (type == null)
                        {
                            continue;
                        }
                        try
                        {
                            var exCache = ReflectionHelper.GetStaticMember(type, "Instance");

                            cacheTypes += "\r\n" + type;//由于数量不多，这里使用String，不使用StringBuilder
                            addedTypes.Add(type);
                        }
                        catch (Exception ex)
                        {
                            XYTrace.BaseExceptionLog(new Exceptions.BaseException(ex.Message, ex));
                        }
                    }
                }
            }

            var dt2 = SystemTime.Now;
            var exCacheLog = "注册总用时：{0}ms\r\n自动扫描程序集：{1}个\r\n扩展缓存：{2}".FormatWith((dt2 - dt1).TotalMilliseconds, scanTypesCount, cacheTypes);
            XYTrace.SendCustomLog("自动注册扩展缓存完成", exCacheLog);

            return addedTypes;
        }
    }

    /// <summary>
    /// 某一个领域内的缓存策略集合
    /// </summary>
    internal class CacheStrategyDomainMappingCollection : Dictionary<IBaseObjectCacheStrategy, CacheStrategyDomainMappingItem>
    {
        /// <summary>
        /// 添加或更新缓存策略映射
        /// </summary>
        /// <param name="item"></param>
        public void AddOrUpdate(CacheStrategyDomainMappingItem item)
        {
            var cacheStrategy = item.BaseObjectCacheStrategy();
            base[cacheStrategy] = item;
        }
    }

    /// <summary>
    /// 领域内不同类型缓存 -> 底层（CO2NET 级别） 的映射关系
    /// </summary>
    internal class CacheStrategyDomainMappingItem
    {
        /// <summary>
        /// 扩展缓存策略（例如RedisContainerCacheStrategy）
        /// </summary>
        public IDomainExtensionCacheStrategy DomainExtensionCacheStrategy { get; set; }

        /// <summary>
        /// 扩展缓存策略所使用的底层缓存策略（如RedisCacheStrategy）
        /// </summary>
        public Func<IBaseObjectCacheStrategy> BaseObjectCacheStrategy { get { return DomainExtensionCacheStrategy.BaseCacheStrategy; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="domainExtensionCacheStrategy">扩展缓存策略（例如RedisContainerCacheStrategy）</param>
        public CacheStrategyDomainMappingItem(IDomainExtensionCacheStrategy domainExtensionCacheStrategy)
        {
            DomainExtensionCacheStrategy = domainExtensionCacheStrategy;
        }
    }
}
