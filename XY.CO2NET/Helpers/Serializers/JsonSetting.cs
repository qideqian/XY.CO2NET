using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace XY.CO2NET.Helpers
{
    /// <summary>
    /// JSON输出设置
    /// </summary>
    public class JsonSetting : JsonSerializerSettings
    {
        /// <summary>
        /// 是否忽略当前类型以及具有IJsonIgnoreNull接口，且为Null值的属性。如果为true，符合此条件的属性将不会出现在Json字符串中
        /// </summary>
        public bool IgnoreNulls { get; set; }
        /// <summary>
        /// 需要特殊忽略null值的属性名称
        /// </summary>
        public List<string> PropertiesToIgnoreNull { get; set; }
        /// <summary>
        /// 指定类型（Class，非Interface）下的为null属性不生成到Json中
        /// </summary>
        public List<Type> TypesToIgnoreNull { get; set; }

        #region Add
        public class IgnoreValueAttribute : System.ComponentModel.DefaultValueAttribute
        {
            public IgnoreValueAttribute(object value) : base(value)
            {
                //Value = value;
            }
        }
        public class IgnoreNullAttribute : Attribute
        {

        }
        /// <summary>
        /// 例外属性，即不排除的属性值
        /// </summary>
        public class ExcludedAttribute : Attribute
        {

        }

        /// <summary>
        /// 枚举类型显示字符串
        /// </summary>
        public class EnumStringAttribute : Attribute
        {

        }

        #endregion
        /// <summary>
        /// JSON 输出设置 构造函数
        /// </summary>
        /// <param name="ignoreNulls">是否忽略当前类型以及具有IJsonIgnoreNull接口，且为Null值的属性。如果为true，符合此条件的属性将不会出现在Json字符串中</param>
        /// <param name="propertiesToIgnoreNull">需要特殊忽略null值的属性名称</param>
        /// <param name="typesToIgnoreNull">指定类型（Class，非Interface）下的为null属性不生成到Json中</param>
        public JsonSetting(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            IgnoreNulls = ignoreNulls;
            PropertiesToIgnoreNull = propertiesToIgnoreNull ?? new List<string>();
            TypesToIgnoreNull = typesToIgnoreNull ?? new List<Type>();
        }
    }

    public class JsonSettingWrap : JsonSerializerSettings
    {
        public JsonSettingWrap() : this(null)
        {

        }

        public JsonSettingWrap(JsonSetting jsonSetting)
        {
            if (jsonSetting != null)
            {
                //如果为null则不进行特殊处理
                ContractResolver = new JsonContractResolver(jsonSetting.IgnoreNulls, jsonSetting.PropertiesToIgnoreNull, jsonSetting.TypesToIgnoreNull);
            }
        }

        /// <summary>
        /// JSON 输出设置 构造函数  优先级： ignoreNulls &lt; propertiesToIgnoreNull &lt; typesToIgnoreNull
        /// </summary>
        /// <param name="ignoreNulls">是否忽略具有IJsonIgnoreNull接口，且为Null值的属性。如果为true，符合此条件的属性将不会出现在Json字符串中</param>
        /// <param name="propertiesToIgnoreNull">需要特殊忽略null值的属性名称</param>
        /// <param name="typesToIgnoreNull">指定类型（Class，非Interface）下的为null属性不生成到Json中</param>
        public JsonSettingWrap(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            ContractResolver = new JsonContractResolver(ignoreNulls, propertiesToIgnoreNull, typesToIgnoreNull);
        }

    }
    public class JsonContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// 是否忽略当前类型以及具有IJsonIgnoreNull接口，且为Null值的属性。如果为true，符合此条件的属性将不会出现在Json字符串中
        /// </summary>
        bool IgnoreNulls;
        /// <summary>
        /// 需要特殊忽略null值的属性名称
        /// </summary>
        public List<string> PropertiesToIgnoreNull { get; set; }
        /// <summary>
        /// 指定类型（Class，非Interface）下的为null属性不生成到Json中
        /// </summary>
        public List<Type> TypesToIgnoreNull { get; set; }
        /// <summary>
        /// JSON 输出设置 构造函数  优先级： ignoreNulls &lt; propertiesToIgnoreNull &lt; typesToIgnoreNull
        /// </summary>
        /// <param name="ignoreNulls">是否忽略当前类型以及具有IJsonIgnoreNull接口，且为Null值的属性。如果为true，符合此条件的属性将不会出现在Json字符串中</param>
        /// <param name="propertiesToIgnoreNull">需要特殊忽略null值的属性名称</param>
        /// <param name="typesToIgnoreNull">指定类型（Class，非Interface）下的为null属性不生成到Json中</param>
        public JsonContractResolver(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            IgnoreNulls = ignoreNulls;
            PropertiesToIgnoreNull = propertiesToIgnoreNull;
            TypesToIgnoreNull = typesToIgnoreNull;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            //TypesToIgnoreNull指定类型（Class，非Interface）下的为null属性不生成到Json中
            if (TypesToIgnoreNull.Contains(type))
            {
                type.IsDefined(typeof(JsonSetting.IgnoreNullAttribute), false);
            }
            return base.CreateProperties(type, memberSerialization);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

#if NET48
            //IgnoreNull标注的字段根据IgnoreNulls设定是否序列化
            var ignoreNull = member.GetCustomAttribute<JsonSetting.IgnoreNullAttribute>();
            if (ignoreNull != null || IgnoreNulls)
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }
            else
            {
                property.NullValueHandling = NullValueHandling.Include;
            }

            //propertiesToIgnoreNull指定字段为Null时不序列化
            if (PropertiesToIgnoreNull.Contains(member.Name))
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }

            //枚举序列化
            var enumString = member.GetCustomAttribute<JsonSetting.EnumStringAttribute>();
            if (enumString != null)
            {
                property.Converter = new StringEnumConverter();
            }
#else
            var customAttributes = member.GetCustomAttributes(false);
            var ignoreNullAttribute = typeof(JsonSetting.IgnoreNullAttribute);
            //IgnoreNull标注的字段根据IgnoreNulls设定是否序列化
            if (IgnoreNulls || customAttributes.Count(o => o.GetType() == ignoreNullAttribute) == 1)
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }
            else
            {
                property.NullValueHandling = NullValueHandling.Include;
            }

            //PropertiesToIgnoreNull指定字段为Null时不序列化
            if (PropertiesToIgnoreNull.Contains(member.Name))
            {
                property.NullValueHandling = NullValueHandling.Ignore;
            }

            //TypesToIgnoreNull特定类型字段为Null时不序列化
            if (TypesToIgnoreNull.Contains(property.PropertyType))
            {
                //Console.WriteLine("忽略null值：" + property.PropertyType);
                property.NullValueHandling = NullValueHandling.Ignore;//这样设置无效
                var t = member.DeclaringType;
                property.ShouldSerialize = instance =>
                {
                    try
                    {
                        var value = (member as PropertyInfo).GetValue(instance, null);
                        return value != null;
                    }
                    catch (Exception ex)
                    {
                        Trace.XYTrace.BaseExceptionLog(new Exceptions.BaseException(ex.Message, ex));
                        return true;
                    }
                };
            }

            //符合IgnoreValue标注值的字段不序列化
            var ignoreValueAttribute = typeof(JsonSetting.IgnoreValueAttribute);
            var ignoreValue = customAttributes.FirstOrDefault(o => o.GetType() == ignoreValueAttribute);
            if (ignoreValue != null)
            {
                //property.DefaultValueHandling = DefaultValueHandling.Ignore;
                var t = member.DeclaringType;

                property.ShouldSerialize = instance =>
                {
                    var value = (member as PropertyInfo).GetValue(instance, null);
                    return !value.Equals((ignoreValue as JsonSetting.IgnoreValueAttribute).Value);
                };
            }

            //枚举序列化
            var enumStringAttribute = typeof(JsonSetting.EnumStringAttribute);
            if (customAttributes.Count(o => o.GetType() == enumStringAttribute) == 1)
            {
                property.Converter = new StringEnumConverter();
            }
#endif
            return property;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            return base.CreateContract(objectType);
        }
    }
}
