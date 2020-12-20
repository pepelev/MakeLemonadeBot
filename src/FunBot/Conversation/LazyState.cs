using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Conversation
{
    public sealed class LazyState : State
    {
        private readonly Lazy<State> state;

        public LazyState(Func<State> state)
        {
            this.state = new Lazy<State>(state);
        }

        public override async Task<State> RespondAsync(string query) => await state.Value.RespondAsync(query);
        public override async Task<State> ExpireAsync() => await state.Value.ExpireAsync();
        public override DateTime ExpiresAt => state.Value.ExpiresAt;
        public override JObject Serialize() => state.Value.Serialize();
    }
}