using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class Feedback : Conversation
    {
        private readonly Talk talk;
        private readonly Conversation back;

        public Feedback(Talk talk, Conversation back)
        {
            this.back = back;
            this.talk = talk;
        }

        public override async Task<Conversation> AnswerAsync(string query)
        {
            if (query.ToLowerInvariant() == "я передумал")
            {
                await talk.SayAsync("Ничего, напиши когда надумаешь");
                return back;
            }

            Console.WriteLine($"Feedback {query}");
            await talk.SayAsync("Спасибо!");
            return back;
        }

        public override Task<Conversation> AskAsync() => Task.FromResult<Conversation>(this);
        public override DateTime AskAt => Ask.Never;

        public override JObject Serialize() => new JObject(
            new JProperty("type", "feedback"),
            new JProperty("from", back.Serialize())
        );
    }
}