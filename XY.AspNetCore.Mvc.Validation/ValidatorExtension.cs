using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller 参数验证扩展方法。
/// </summary>
public static class ValidatorExtension
{
    private static readonly Regex InvalidUserInfoRegex = new Regex(
        @"^\s*$|^c:\\con\\con$|[%,\*""\s\t\<\>\&]|游客|管理员|^Guest|admin",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex SqlDangerousCharsRegex = new Regex(
        @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// 创建验证容器。
    /// </summary>
    public static ValidatorContainer<T> Validator<T>(this ControllerBase controller, T validatorObject, string valueName, string htmlName)
    {
        return Validator(controller, validatorObject, valueName, htmlName, null);
    }

    /// <summary>
    /// 创建验证容器并可选执行空值校验。
    /// </summary>
    public static ValidatorContainer<T> Validator<T>(
        this ControllerBase controller,
        T validatorObject,
        string valueName,
        string htmlName,
        bool? nullOrEmptyable = null)
    {
        if (controller.ModelState[htmlName] != null && controller.ModelState[htmlName]?.Errors.Count > 0)
        {
            controller.ModelState[htmlName]?.Errors.Clear();
        }

        var isEmpty = validatorObject == null || string.IsNullOrEmpty(validatorObject.ToString()?.Trim());
        if (nullOrEmptyable != null && isEmpty)
        {
            if (nullOrEmptyable == true)
            {
                return default!;
            }

            var container = new ValidatorContainer<T>(controller, validatorObject, valueName, htmlName);
            return container.NotNullOrEmpty(true);
        }

        return new ValidatorContainer<T>(controller, validatorObject, valueName, htmlName);
    }

    public static ValidatorContainer<T> NotNullOrEmpty<T>(this ValidatorContainer<T> container)
    {
        return NotNullOrEmpty(container, true);
    }

    public static ValidatorContainer<T> NotNullOrEmpty<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (container.ValidatorObject == null || string.IsNullOrEmpty(container.ValidatorObject.ToString()?.Trim()))
        {
            var errorMessage = $"请填写 {container.ValueName}!";
            container.AddError(errorMessage);

            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsTrue<T>(this ValidatorContainer<T> container, Func<T, bool> func, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (!func.Invoke(container.ValidatorObject))
        {
            var errorMessage = string.Format(failMessageFormat, container.ValueName);
            container.AddError(errorMessage);
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsFalse<T>(this ValidatorContainer<T> container, Func<T, bool> func, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (func.Invoke(container.ValidatorObject))
        {
            var errorMessage = string.Format(failMessageFormat, container.ValueName);
            container.AddError(errorMessage);
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> Fail<T>(this ValidatorContainer<T> container, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        var errorMessage = string.Format(failMessageFormat, container.ValueName);
        container.AddError(errorMessage);
        if (stopWhileFail)
        {
            return default!;
        }

        return container;
    }

    public static ValidatorContainer<T> Regex<T>(this ValidatorContainer<T> container, string expression, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        var regex = new Regex(expression, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (!regex.IsMatch(container.ValidatorObject?.ToString() ?? string.Empty))
        {
            var errorMessage = string.Format(failMessageFormat, container.ValueName);
            container.AddError(errorMessage);
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsSafeSqlString<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (SqlDangerousCharsRegex.IsMatch(container.ValidatorObject?.ToString() ?? string.Empty))
        {
            container.AddError($"{container.ValueName}中存在非法字符！");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsSafeUserInfoString<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (InvalidUserInfoRegex.IsMatch(container.ValidatorObject?.ToString() ?? string.Empty))
        {
            container.AddError($"{container.ValueName}中存在非法字符！");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsValidateUserInfoName<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        var userName = container.ValidatorObject?.ToString() ?? string.Empty;
        if (userName.Contains('　') || userName.Contains('') || userName.Contains('') || userName.Contains('') || userName.Contains('') ||
            userName.Contains('') || userName.Contains('') || userName.Contains('') || userName.Contains('') || userName.Contains('') ||
            userName.Contains(''))
        {
            container.AddError("用户名中不允许包含全角空格符");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        if (userName.Contains(' '))
        {
            container.AddError("用户名中不允许包含空格");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        if (userName.Contains(':'))
        {
            container.AddError("用户名中不允许包含冒号");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        const string invalidateUserName = "`~!@#$%^&*()+-=;':\",./<>?|\\";
        foreach (var item in invalidateUserName)
        {
            if (userName.Contains(item))
            {
                container.AddError("用户名中可以使用中文、英文或数字及下划线_，但不允许包含特殊符号：" + invalidateUserName);
                if (stopWhileFail)
                {
                    return default!;
                }
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsEmail<T>(this ValidatorContainer<T> container)
    {
        return IsEmail(container, true);
    }

    public static ValidatorContainer<T> IsEmail<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        return Regex(container,
            @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
            "请在{0}中填写正确的Email地址！",
            stopWhileFail);
    }

    public static ValidatorContainer<T> IsEnum<T>(this ValidatorContainer<T> container, Type enumType, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (!Enum.IsDefined(enumType, container.ValidatorObject!))
        {
            container.AddError($"{container.ValueName}中选择的值不在列表中");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsDateTime<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        return Regex(container,
            @"^\d{4}/\d{1,2}/\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}$",
            "请在{0}中填写正确的时间格式",
            stopWhileFail);
    }

    public static ValidatorContainer<T> IsMobile<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        return Regex(container, @"^1[356789]\d{9}$", "请填写正确的电话号码！", stopWhileFail);
    }

    public static ValidatorContainer<T> IsIPAddress<T>(this ValidatorContainer<T> container)
    {
        return IsIPAddress(container, true);
    }

    public static ValidatorContainer<T> IsIPAddress<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        return Regex(container,
            @"(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])",
            "请在{0}中填写正确的IP地址！",
            stopWhileFail);
    }

    public static ValidatorContainer<T> IsAvailableUrl<T>(this ValidatorContainer<T> container, bool stopWhileFail)
    {
        return Regex(container,
            @"HTTP(S)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?",
            "请在{0}中填写正确的Url地址！",
            stopWhileFail);
    }

    public static ValidatorContainer<T> MaxByte<T>(this ValidatorContainer<T> container, int maxByte)
    {
        return MaxByte(container, maxByte, true);
    }

    public static ValidatorContainer<T> MaxByte<T>(this ValidatorContainer<T> container, int maxByte, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (Encoding.Default.GetByteCount(container.ValidatorObject?.ToString() ?? string.Empty) > maxByte)
        {
            var errorMessage = $"{container.ValueName}中最多只能输入{maxByte}字节的内容（对应{maxByte / 2}汉字）";
            container.AddError(errorMessage);
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> MaxLength<T>(this ValidatorContainer<T> container, int maxLength)
    {
        return MaxLength(container, maxLength, true);
    }

    public static ValidatorContainer<T> MaxLength<T>(this ValidatorContainer<T> container, int maxLength, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if ((container.ValidatorObject?.ToString()?.Length ?? 0) > maxLength)
        {
            container.AddError($"{container.ValueName}中最多只能输入{maxLength}个字符");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> MinByte<T>(this ValidatorContainer<T> container, int minByte)
    {
        return MinByte(container, minByte, true);
    }

    public static ValidatorContainer<T> MinByte<T>(this ValidatorContainer<T> container, int minByte, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (Encoding.Default.GetByteCount(container.ValidatorObject?.ToString() ?? string.Empty) < minByte)
        {
            var errorMessage = $"{container.ValueName}中至少需要输入{minByte}字节的内容（对应{minByte / 2}汉字）";
            container.AddError(errorMessage);
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> MinLength<T>(this ValidatorContainer<T> container, int minLength)
    {
        return MinLength(container, minLength, true);
    }

    public static ValidatorContainer<T> MinLength<T>(this ValidatorContainer<T> container, int minLength, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if ((container.ValidatorObject?.ToString()?.Length ?? 0) < minLength)
        {
            container.AddError($"{container.ValueName}中至少需要输入{minLength}个字符");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsEqual<T>(this ValidatorContainer<T> container, T obj, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (!Equals(container.ValidatorObject, obj))
        {
            container.AddError(string.Format(failMessageFormat, container.ValueName));
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsNotEqual<T>(this ValidatorContainer<T> container, T obj, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (Equals(container.ValidatorObject, obj))
        {
            container.AddError(string.Format(failMessageFormat, container.ValueName));
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> Exclude<T>(this ValidatorContainer<T> container, string[] excludeStrings)
    {
        return Exclude(container, excludeStrings, true);
    }

    public static ValidatorContainer<T> Exclude<T>(this ValidatorContainer<T> container, string[] excludeStrings, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        var current = container.ValidatorObject?.ToString() ?? string.Empty;
        foreach (var str in excludeStrings)
        {
            if (current.Contains(str))
            {
                var errorMessage = $"{container.ValueName}中包含了不允许使用的特殊字符：{str}";
                container.AddError(errorMessage);
                if (stopWhileFail)
                {
                    return default!;
                }
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsNumber<T>(this ValidatorContainer<T> container, float? min, float? max, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        min ??= float.MinValue;
        max ??= float.MaxValue;

        var isValid = float.TryParse(container.ValidatorObject?.ToString(), out var tryFloat)
                      && tryFloat >= min.Value
                      && tryFloat <= max.Value;

        if (!isValid)
        {
            container.AddError($"请输入正确的{container.ValueName}");
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsNull<T>(this ValidatorContainer<T> container, object obj, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (obj != null)
        {
            container.AddError(string.Format(failMessageFormat, container.ValueName));
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }

    public static ValidatorContainer<T> IsNotNull<T>(this ValidatorContainer<T> container, object obj, string failMessageFormat, bool stopWhileFail)
    {
        if (container == null)
        {
            return default!;
        }

        if (obj == null)
        {
            container.AddError(string.Format(failMessageFormat, container.ValueName));
            if (stopWhileFail)
            {
                return default!;
            }
        }

        return container;
    }
}
