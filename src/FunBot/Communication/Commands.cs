using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class Commands : Interaction<string, Conversation>
    {
        private readonly ImmutableStack<(string Command, Content.Collection Collection)> commands;
        private readonly Conversation back;
        private readonly Conversation empty;
        private readonly Conversation selected;
        private readonly Talk talk;
        private readonly int queriesLeft;

        public Commands(
            ImmutableStack<(string Command, Content.Collection Collection)> commands,
            Conversation back,
            Conversation selected,
            Conversation empty,
            Talk talk,
            int queriesLeft)
        {
            this.commands = commands;
            this.back = back;
            this.selected = selected;
            this.empty = empty;
            this.talk = talk;
            this.queriesLeft = queriesLeft;
        }

        public override async Task<Conversation> RunAsync(string query)
        {
            var interaction = Interaction();
            return await interaction.RunAsync(query);
        }

        private Interaction<string, Conversation> Interaction()
        {
            if (commands.IsEmpty)
            {
                return new WithoutArgument<string, Conversation>(
                    new Misunderstanding(talk, back)
                );
            }

            var (command, collection) = commands.Peek();
            return new Matching<string, Conversation>(
                command,
                StringComparer.InvariantCultureIgnoreCase,
                new Limited<string>(
                    queriesLeft,
                    talk,
                    empty,
                    new WithoutArgument<string, Conversation>(
                        new Show(
                            collection,
                            talk,
                            back,
                            selected
                        )
                    )
                ),
                new Commands(
                    commands.Pop(),
                    back,
                    selected,
                    empty,
                    talk,
                    queriesLeft
                )
            );
        }
    }
}