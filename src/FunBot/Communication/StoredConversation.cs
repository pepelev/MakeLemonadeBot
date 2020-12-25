using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using FunBot.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class StoredConversation : Conversation
    {
        private readonly long chatId;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly Conversation conversation;
        private readonly Factory factory;
        private readonly JObject @object;
        private readonly Random random;
        private readonly Talk.Collection talks;

        public StoredConversation(
            long chatId,
            Talk.Collection talks,
            Clock clock,
            SQLiteConnection connection,
            Random random,
            Factory factory,
            JObject @object)
        {
            this.chatId = chatId;
            this.talks = talks;
            this.clock = clock;
            this.connection = connection;
            this.random = random;
            this.factory = factory;
            this.@object = @object;
            conversation = new LazyConversation(Parse);
        }

        public override DateTime AskAt => conversation.AskAt;

        private Conversation Parse() => Parse(@object);

        private Keyboard FullKeyboard => new TwoColumnKeyboard(
            "Кино",
            "Сериалы",
            //"Мультфильмы",
            "Книги",
            //"Кладовая",
            "Написать нам"
        );

        private Conversation Parse(JObject json)
        {
            var type = json.Get<string>("type");
            return type switch
            {
                "greeting" => new Greeting(
                    talks.For(chatId, FullKeyboard),
                    new LazyConversation(() => factory.Selection(5))
                ),
                "selection" => Selection(json),
                "serialSelection" => SerialSelection(json),
                "feedback" => factory.Feedback(
                    Parse(json.Get<JObject>("from"))
                ),
                _ => throw new Exception($"Could not parse {@object}")
            };
        }

        private Conversation Selection(JObject json)
        {
            var left = json.Get<int>("queriesLeft");
            return factory.Selection(left);
        }

        private Conversation SerialSelection(JObject json)
        {
            var left = json.Get<int>("queriesLeft");
            return factory.SerialSelection(left);
        }

        public override async Task<Conversation> AnswerAsync(string query) =>
            await conversation.AnswerAsync(query);

        public override async Task<Conversation> AskAsync() => await conversation.AskAsync();
        public override JObject Serialize() => @object;
    }
}