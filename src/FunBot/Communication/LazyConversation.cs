using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class LazyConversation : Conversation
    {
        private readonly Lazy<Conversation> state;

        public LazyConversation(Func<Conversation> state)
        {
            this.state = new Lazy<Conversation>(state);
        }

        public override async Task<Conversation> AnswerAsync(string query) => await state.Value.AnswerAsync(query);
        public override async Task<Conversation> AskAsync() => await state.Value.AskAsync();
        public override DateTime AskAt => state.Value.AskAt;
        public override JObject Serialize() => state.Value.Serialize();
    }
}