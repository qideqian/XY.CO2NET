using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XY.CO2NET.Helpers
{
    /// <summary>
    /// JSON 自定义输出选项；由 JsonSettingWrap 创建 JsonContractResolver 后应用这些选项。
    /// </summary>
    public class JsonSetting : JsonSerializerSettings
    {
        /// <summary>
        /// 全局空值忽略开关；为 false 时，成员上的 IgnoreNullAttribute 以及名称或类型规则仍可要求忽略空值。
        /// </summary>
        public bool IgnoreNulls { get; set; }
        /// <summary>
        /// 按反射成员名称匹配的空值忽略名单（不是重命名后的 JSON 名称）
        /// </summary>
        public List<string> PropertiesToIgnoreNull { get; set; }
        /// <summary>
        /// 未定义 net48 编译符号时，按 JSON 属性的 PropertyType 精确匹配空值忽略规则；不是按所属对象类型匹配。net48 的 CreateProperty 分支不使用此类型规则。
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
        /// 例外标记类型；本文件的 JsonContractResolver 未读取此标记，不据此取消其他忽略规则。
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
        /// <param name="ignoreNulls">全局空值忽略开关；为 false 时，成员上的 IgnoreNullAttribute 以及名称或类型规则仍可要求忽略空值。</param>
        /// <param name="propertiesToIgnoreNull">按反射成员名称匹配的空值忽略名单（不是重命名后的 JSON 名称）</param>
        /// <param name="typesToIgnoreNull">未定义 net48 编译符号时，按 JSON 属性的 PropertyType 精确匹配空值忽略规则；不是按所属对象类型匹配。net48 的 CreateProperty 分支不使用此类型规则。</param>
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

        /// <summary>仅从自定义选项创建解析器，不复制 JsonSetting 继承的其他 JsonSerializerSettings 设置。</summary>
        /// <param name="jsonSetting">为 null 时不设置 ContractResolver；非 null 时读取其三个自定义选项。</param>
        public JsonSettingWrap(JsonSetting jsonSetting)
        {
            if (jsonSetting != null)
            {
                //如果为null则不进行特殊处理
                ContractResolver = new JsonContractResolver(jsonSetting.IgnoreNulls, jsonSetting.PropertiesToIgnoreNull, jsonSetting.TypesToIgnoreNull);
            }
        }

        /// <summary>
        /// 创建应用空值、成员名称和成员类型规则的解析器；这些规则可共同要求忽略空值。
        /// </summary>
        /// <param name="ignoreNulls">全局空值忽略开关；为 false 时，成员上的 IgnoreNullAttribute 以及名称或类型规则仍可要求忽略空值。</param>
        /// <param name="propertiesToIgnoreNull">按反射成员名称匹配的空值忽略名单（不是重命名后的 JSON 名称）</param>
        /// <param name="typesToIgnoreNull">未定义 net48 编译符号时，按 JSON 属性的 PropertyType 精确匹配空值忽略规则；不是按所属对象类型匹配。net48 的 CreateProperty 分支不使用此类型规则。</param>
        /// <remarks>直接调用此构造函数时，两个名单应传入非 null 集合；本构造函数不补建集合，后续解析会调用它们的 Contains。</remarks>
        public JsonSettingWrap(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            ContractResolver = new JsonContractResolver(ignoreNulls, propertiesToIgnoreNull, typesToIgnoreNull);
        }

    }
    public class JsonContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// 全局空值忽略开关；为 false 时，成员上的 IgnoreNullAttribute 以及名称或类型规则仍可要求忽略空值。
        /// </summary>
        bool IgnoreNulls;
        /// <summary>
        /// 按反射成员名称匹配的空值忽略名单（不是重命名后的 JSON 名称）
        /// </summary>
        public List<string> PropertiesToIgnoreNull { get; set; }
        /// <summary>
        /// 未定义 net48 编译符号时，按 JSON 属性的 PropertyType 精确匹配空值忽略规则；不是按所属对象类型匹配。net48 的 CreateProperty 分支不使用此类型规则。
        /// </summary>
        public List<Type> TypesToIgnoreNull { get; set; }
        /// <summary>
        /// 创建应用空值、成员名称和成员类型规则的解析器；这些规则可共同要求忽略空值。
        /// </summary>
        /// <param name="ignoreNulls">全局空值忽略开关；为 false 时，成员上的 IgnoreNullAttribute 以及名称或类型规则仍可要求忽略空值。</param>
        /// <param name="propertiesToIgnoreNull">按反射成员名称匹配的空值忽略名单（不是重命名后的 JSON 名称）</param>
        /// <param name="typesToIgnoreNull">未定义 net48 编译符号时，按 JSON 属性的 PropertyType 精确匹配空值忽略规则；不是按所属对象类型匹配。net48 的 CreateProperty 分支不使用此类型规则。</param>
        /// <remarks>直接调用此构造函数时，两个名单应传入非 null 集合；本构造函数不补建集合，后续解析会调用它们的 Contains。</remarks>
        public JsonContractResolver(bool ignoreNulls = false, List<string> propertiesToIgnoreNull = null, List<Type> typesToIgnoreNull = null)
        {
            IgnoreNulls = ignoreNulls;
            PropertiesToIgnoreNull = propertiesToIgnoreNull;
            TypesToIgnoreNull = typesToIgnoreNull;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            //此处查询所属类型的标记，但未使用查询结果改变属性集合。
            if (TypesToIgnoreNull.Contains(type))
            {
                type.IsDefined(typeof(JsonSetting.IgnoreNullAttribute), false);
            }
            return base.CreateProperties(type, memberSerialization);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

#if net48
            //全局开关或成员上的 IgnoreNullAttribute 均可启用空值忽略。
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
            //全局开关或成员上的 IgnoreNullAttribute 均可启用空值忽略。
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

            //此分支按 JSON 属性的值类型匹配，并设置非 null 值才序列化的判断。
            if (TypesToIgnoreNull.Contains(property.PropertyType))
            {
                //Console.WriteLine("忽略null值：" + property.PropertyType);
                property.NullValueHandling = NullValueHandling.Ignore;//设置空值忽略，并在下方补充 ShouldSerialize。
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

            //IgnoreValue 在此分支设置 ShouldSerialize，会覆盖前面的类型规则所设置的判断。
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
