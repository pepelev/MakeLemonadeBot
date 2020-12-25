using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class Commands : Interaction<string, State>
    {
        private readonly ImmutableStack<(string Command, Content.Collection Collection)> commands;
        private readonly State.Factory factory;
        private readonly Talk talk;
        private readonly int queriesLeft;

        public Commands(
            ImmutableStack<(string Command, Content.Collection Collection)> commands,
            State.Factory factory,
            Talk talk,
            int queriesLeft)
        {
            this.commands = commands;
            this.factory = factory;
            this.talk = talk;
            this.queriesLeft = queriesLeft;
        }

        public override async Task<State> RunAsync(string query)
        {
            var interaction = Interaction();
            return await interaction.RunAsync(query);
        }

        private Interaction<string, State> Interaction()
        {
            if (commands.IsEmpty)
            {
                return new WithoutArgument<string, State>(
                    new Misunderstanding(talk, factory.Selection(queriesLeft))
                );
            }

            var (command, collection) = commands.Peek();
            return new Matching<string, State>(
                command,
                StringComparer.InvariantCultureIgnoreCase,
                new Limited<string>(
                    queriesLeft,
                    talk,
                    factory,
                    new WithoutArgument<string, State>(
                        new Show(
                            collection,
                            talk,
                            factory.Selection(queriesLeft),
                            factory.Selection(queriesLeft - 1)
                        )
                    )
                ),
                new Commands(
                    commands.Pop(),
                    factory,
                    talk,
                    queriesLeft
                )
            );
        }
    }
}