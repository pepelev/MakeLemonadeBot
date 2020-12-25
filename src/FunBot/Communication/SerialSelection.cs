using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FunBot.Communication
{
    public sealed class SerialSelection : State
    {
        private readonly int queriesLeft;
        private readonly Content.Collection @long;
        private readonly Content.Collection @short;
        private readonly Talk talk;
        private readonly State back;
        private readonly State next;

        public SerialSelection(
            Talk talk,
            Content.Collection @short,
            Content.Collection @long,
            State back,
            State next,
            int queriesLeft)
        {
            this.talk = talk;
            this.@short = @short;
            this.@long = @long;
            this.back = back;
            this.next = next;
            this.queriesLeft = queriesLeft;
        }

        public override DateTime ExpiresAt => Expires.Never;

        public override async Task<State> RespondAsync(string query)
        {
            var interaction = new Matching<string, State>(
                "длинный",
                StringComparer.InvariantCultureIgnoreCase,
                new WithoutArgument<string, State>(
                    new Show(
                        @long,
                        talk,
                        back,
                        next
                    )
                ),
                new Matching<string, State>(
                    "короткий",
                    StringComparer.InvariantCultureIgnoreCase,
                    new WithoutArgument<string, State>(
                        new Show(
                            @short,
                            talk,
                            back,
                            next
                        )
                    ),
                    new WithoutArgument<string, State>(
                        new Misunderstanding(
                            talk,
                            back
                        )
                    )
                )
            );
            return await interaction.RunAsync(query);
        }

        public override Task<State> ExpireAsync() => Task.FromResult<State>(this);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "serialSelection"),
            new JProperty("queriesLeft", queriesLeft)
        );
    }
}