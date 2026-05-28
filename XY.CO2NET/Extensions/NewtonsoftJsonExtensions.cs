using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Json扩展
    /// </summary>
    public static class JsonExtensions
    {
        public static string TryGetString(this JToken token, string propertyName)
        {
            if (!token.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            return value.Type switch
            {
                JTokenType.String => value.Value<string>(),
                JTokenType.Integer => value.ToString(Formatting.None),
                JTokenType.Float => value.ToString(Formatting.None),
                JTokenType.Boolean => value.Value<bool>() ? bool.TrueString : bool.FalseString,
                _ => null
            };
        }

        public static bool? TryGetBoolean(this JToken token, string propertyName)
        {
            if (!token.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            if (value.Type == JTokenType.Boolean)
            {
                return value.Value<bool>();
            }

            if (value.Type == JTokenType.String && bool.TryParse(value.Value<string>(), out var parsed))
            {
                return parsed;
            }

            return null;
        }

        public static int? TryGetInt32(this JToken token, string propertyName)
        {
            if (!token.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            if (value.Type == JTokenType.Integer && int.TryParse(value.ToString(Formatting.None), out var number))
            {
                return number;
            }

            if (value.Type == JTokenType.String && int.TryParse(value.Value<string>(), out number))
            {
                return number;
            }

            return null;
        }

        private static bool TryGetProperty(this JToken token, string propertyName, out JToken value)
        {
            value = null;

            if (!(token is JObject obj))
            {
                return false;
            }

            return obj.TryGetValue(propertyName, StringComparison.Ordinal, out value);
        }
    }
}
