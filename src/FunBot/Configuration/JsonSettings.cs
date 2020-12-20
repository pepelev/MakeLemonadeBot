using FunBot.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Configuration
{
    public sealed partial class JsonSettings : Settings
    {
        private readonly JObject @object;

        public JsonSettings(JObject @object)
        {
            this.@object = @object;
        }

        public override Configuration.TelegramSettings Telegram => new TelegramSettings(
            @object.Get<JObject>("telegram")
        );

        public override Source.Collection Sources => new SourceCollection(
            @object.Get<JObject>("sources")
        );

        public override User.Collection Users => new UserCollection(
            @object.Get<JObject>("users")
        );
    }
}