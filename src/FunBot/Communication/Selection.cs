using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class Selection : Conversation
    {
        private readonly Conversation expiredConversation;
        private readonly Interaction<string, Conversation> interaction;
        private readonly int queriesLeft;
        private readonly DateTime timestamp;

        public Selection(
            DateTime timestamp,
            int queriesLeft,
            Conversation expiredConversation,
            Interaction<string, Conversation> interaction)
        {
            this.timestamp = timestamp;
            this.queriesLeft = queriesLeft;
            this.expiredConversation = expiredConversation;
            this.interaction = interaction;
        }

        public override DateTime AskAt => timestamp.Date + new TimeSpan(TimeSpan.TicksPerDay);

        public override async Task<Conversation> AnswerAsync(string query)
        {
            return await interaction.RunAsync(query);
        }

        public override Task<Conversation> AskAsync() => Task.FromResult(expiredConversation);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "selection"),
            new JProperty("queriesLeft", queriesLeft),
            new JProperty("timestamp", timestamp.ToString("O"))
        );
    }
}