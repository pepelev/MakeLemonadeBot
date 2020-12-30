using MakeLemonadeBot.Json;
using Newtonsoft.Json.Linq;

namespace MakeLemonadeBot.Configuration
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

        public override Phrases Phrases
        {
            get
            {
                var phrases = @object.Get("phrases", new JObject());
                return new PhraseCollection(phrases);
            }
        }

        private sealed class PhraseCollection : Phrases
        {
            private readonly JObject @object;

            public PhraseCollection(JObject @object)
            {
                this.@object = @object;
            }

            public override string Greeting => @object.Get("greeting", "Привет, это отличный бот");
        }
    }
}