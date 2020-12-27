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

            public override TelegramListeningSettings Listening => new ListeningSettings(
                @object.Get<JObject>("listening")
            );

            public override TelegramDestinationSettings Feedback => new Destination(
                @object.Get<JObject>("feedback")
            );

            public override TelegramDestinationSettings Log => new Destination(
                @object.Get<JObject>("log")
            );

            private sealed class ListeningSettings : TelegramListeningSettings
            {
                private readonly JObject @object;

                public ListeningSettings(JObject @object)
                {
                    this.@object = @object;
                }

                public override string Token => @object.Get<string>("token");
            }

            private sealed class Destination : TelegramDestinationSettings
            {
                private readonly JObject @object;

                public Destination(JObject @object)
                {
                    this.@object = @object;
                }

                public override string Token => @object.Get<string>("token");
                public override string ChatId => @object.Get<string>("chatId");
            }
        }
    }
}