using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MakeLemonadeBot.Communication
{
    public sealed class FullSelection : Conversation
    {
        private readonly Interaction<string, Conversation> interaction;

        public FullSelection(Interaction<string, Conversation> interaction)
        {
            this.interaction = interaction;
        }

        public override DateTime AskAt => Ask.Never;

        public override async Task<Conversation> AnswerAsync(string query)
        {
            return await interaction.RunAsync(query);
        }

        public override Task<Conversation> AskAsync() => Task.FromResult(this as Conversation);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "fullSelection")
        );
    }
}