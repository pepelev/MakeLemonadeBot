using System;
using System.Threading.Tasks;
using FunBot.Configuration;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class Greeting : Conversation
    {
        private readonly Conversation next;
        private readonly Phrases phrases;
        private readonly Talk talk;

        public Greeting(Talk talk, Phrases phrases, Conversation next)
        {
            this.talk = talk;
            this.next = next;
            this.phrases = phrases;
        }

        public override DateTime AskAt => Ask.Never;

        public override async Task<Conversation> AnswerAsync(string query)
        {
            await talk.SayAsync(phrases.Greeting);
            return next;
        }

        public override Task<Conversation> AskAsync() => Task.FromResult<Conversation>(this);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "greeting")
        );
    }
}