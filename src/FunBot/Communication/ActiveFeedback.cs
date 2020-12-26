using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class ActiveFeedback : Conversation
    {
        private readonly Conversation conversation;
        private readonly DateTime firstQueryTimestamp;
        private readonly Talk talk;
        private readonly Conversation feedback;

        public ActiveFeedback(
            DateTime firstQueryTimestamp,
            Talk talk,
            Conversation conversation,
            Conversation feedback)
        {
            this.firstQueryTimestamp = firstQueryTimestamp;
            this.talk = talk;
            this.conversation = conversation;
            this.feedback = feedback;
        }

        private DateTime Ask => firstQueryTimestamp + new TimeSpan(TimeSpan.TicksPerDay * 7);
        public override DateTime AskAt => Compare.Min(conversation.AskAt, Ask);

        public override async Task<Conversation> AnswerAsync(string query)
        {
            return await conversation.AnswerAsync(query);
        }

        public override async Task<Conversation> AskAsync()
        {
            if (conversation.AskAt < Ask)
            {
                return await conversation.AskAsync();
            }

            await talk.SayAsync("Хей, ты уже 7 дней пользуешься ботом, не хочешь поделиться своими впечатлениями?");
            return feedback;
        }

        public override JObject Serialize() => new JObject(
            new JProperty("type", "activeFeedback"),
            new JProperty("firstQueryTimestamp", firstQueryTimestamp.ToString("O")),
            new JProperty("conversation", conversation.Serialize())
        );
    }
}