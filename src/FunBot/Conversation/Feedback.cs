using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Conversation
{
    public sealed class Feedback : State
    {
        private readonly Talk talk;
        private readonly State back;

        public Feedback(Talk talk, State back)
        {
            this.back = back;
            this.talk = talk;
        }

        public override async Task<State> RespondAsync(string query)
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

        public override Task<State> ExpireAsync() => Task.FromResult<State>(this);
        public override DateTime ExpiresAt => Expires.Never;

        public override JObject Serialize() => new JObject(
            new JProperty("type", "feedback"),
            new JProperty("from", back.Serialize())
        );
    }
}