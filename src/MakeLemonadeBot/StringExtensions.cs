using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MakeLemonadeBot
{
    public static class StringExtensions
    {
        public static int? AsNumber(this string? text)
        {
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        public static JObject AsJsonObject(this string jsonText)
        {
            using var text = new StringReader(jsonText);
            using var json = new JsonTextReader(text);
            return JObject.Load(json);
        }
    }
}