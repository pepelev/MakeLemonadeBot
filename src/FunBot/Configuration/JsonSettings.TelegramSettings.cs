using FunBot.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Configuration
{
    public sealed partial class JsonSettings
    {
        private sealed class TelegramSettings : Configuration.TelegramSettings
        {
            private readonly JObject @object;

            public TelegramSettings(JObject @object)
            {
                this.@object = @object;
            }

            public override string Token => @object.Get<string>("token");
        }
    }
}