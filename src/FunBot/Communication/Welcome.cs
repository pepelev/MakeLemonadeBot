using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class Welcome : State
    {
        private readonly Factory factory;
        private readonly Talk talk;

        public Welcome(Talk talk, Factory factory)
        {
            this.talk = talk;
            this.factory = factory;
        }

        public override DateTime ExpiresAt => Expires.Never;

        public override async Task<State> RespondAsync(string query)
        {
            await talk.SayAsync("Привет, это отличный бот");
            return factory.Selection(5);
        }

        public override Task<State> ExpireAsync() => Task.FromResult<State>(this);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "welcome")
        );
    }
}