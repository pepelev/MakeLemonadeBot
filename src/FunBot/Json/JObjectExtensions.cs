using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Json
{
    public static class JObjectExtensions
    {
        public static string AsString(this JObject @object)
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            using var json = new JsonTextWriter(writer);
            @object.WriteTo(json);
            return writer.ToString();
        }

        public static T Get<T>(this JObject @object, string propertyName)
        {
            if (@object.ContainsKey(propertyName))
                return @object.Value<T>(propertyName);

            throw new KeyNotFoundException($"Property {propertyName} not found");
        }
    }
}