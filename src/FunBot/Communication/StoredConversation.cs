using System;
using System.Threading.Tasks;
using FunBot.Json;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class StoredConversation : Conversation
    {
        private readonly Conversation conversation;
        private readonly Factory factory;
        private readonly JObject @object;

        public StoredConversation(Factory factory, JObject @object)
        {
            this.factory = factory;
            this.@object = @object;
            conversation = new LazyConversation(Parse);
        }

        public override DateTime AskAt => conversation.AskAt;

        private Conversation Parse() => Parse(@object);

        private Conversation Parse(JObject json)
        {
            var type = json.Get<string>("type");
            return type switch
            {
                "greeting" => factory.Greeting(),
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