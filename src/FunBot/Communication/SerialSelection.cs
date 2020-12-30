using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MakeLemonadeBot.Communication
{
    public sealed class SerialSelection : Conversation
    {
        private readonly Content.Collection @long;
        private readonly Content.Collection @short;
        private readonly Talk talk;
        private readonly Conversation back;
        private readonly Conversation next;

        public SerialSelection(
            Talk talk,
            Content.Collection @short,
            Content.Collection @long,
            Conversation back,
            Conversation next)
        {
            this.talk = talk;
            this.@short = @short;
            this.@long = @long;
            this.back = back;
            this.next = next;
        }

        public override DateTime AskAt => Ask.Never;

        public override async Task<Conversation> AnswerAsync(string query)
        {
            var interaction = new Matching<string, Conversation>(
                "длинный",
                StringComparer.InvariantCultureIgnoreCase,
                new WithoutArgument<string, Conversation>(
                    new Show(
                        @long,
                        talk,
                        back,
                        next
                    )
                ),
                new Matching<string, Conversation>(
                    "короткий",
                    StringComparer.InvariantCultureIgnoreCase,
                    new WithoutArgument<string, Conversation>(
                        new Show(
                            @short,
                            talk,
                            back,
                            next
                        )
                    ),
                    new WithoutArgument<string, Conversation>(
                        new Misunderstanding(
                            talk,
                            back
                        )
                    )
                )
            );
            return await interaction.RunAsync(query);
        }

        public override Task<Conversation> AskAsync() => Task.FromResult<Conversation>(this);

        public override JObject Serialize() => new JObject(
            new JProperty("type", "serialSelection"),
            new JProperty("back", back.Serialize()),
            new JProperty("next", next.Serialize())
        );
    }
}