using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class Greeting : Conversation
    {
        private readonly Factory factory;
        private readonly Talk talk;

        public Greeting(Talk talk, Factory factory)
        {
            this.talk = talk;
            this.factory = factory;
        }

        public override DateTime AskAt => Expires.Never;

        public override async Task<Conversation> AnswerAsync(string query)
        {
            await talk.SayAsync("Привет, это отличный бот");
            return factory.Selection(5);
        }

        public override Task<Conversation> AskAsync() => Task.FromResult<Conversation>(this);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "greeting")
        );
    }
}