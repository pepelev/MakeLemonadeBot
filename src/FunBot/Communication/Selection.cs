using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class Selection : State
    {
        private readonly State expiredState;
        private readonly Interaction<string, State> interaction;
        private readonly int queriesLeft;
        private readonly DateTime timestamp;

        public Selection(
            DateTime timestamp,
            int queriesLeft,
            State expiredState,
            Interaction<string, State> interaction)
        {
            this.timestamp = timestamp;
            this.queriesLeft = queriesLeft;
            this.expiredState = expiredState;
            this.interaction = interaction;
        }

        public override DateTime ExpiresAt => timestamp.Date + new TimeSpan(TimeSpan.TicksPerDay);

        public override async Task<State> RespondAsync(string query)
        {
            return await interaction.RunAsync(query);
        }

        public override Task<State> ExpireAsync() => Task.FromResult(expiredState);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "selection"),
            new JProperty("queriesLeft", queriesLeft),
            new JProperty("timestamp", timestamp.ToString("O"))
        );
    }
}