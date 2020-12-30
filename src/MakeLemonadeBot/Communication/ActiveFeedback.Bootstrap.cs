using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MakeLemonadeBot.Communication
{
    public sealed partial class ActiveFeedback
    {
        public sealed class Bootstrap : Conversation
        {
            private readonly Conversation conversation;

            public Bootstrap(Conversation conversation)
            {
                this.conversation = conversation;
            }

            public override DateTime AskAt => conversation.AskAt;
            public override async Task<Conversation> AnswerAsync(string query) => await conversation.AnswerAsync(query);
            public override async Task<Conversation> AskAsync() => await conversation.AskAsync();

            public override JObject Serialize() => new JObject(
                new JProperty("type", "activeFeedbackBootstrap"),
                new JProperty("conversation", conversation.Serialize())
            );
        }
    }
}