using System.Threading.Tasks;

namespace FunBot.Conversation
{
    public sealed class Limited<In> : Interaction<In, State>
    {
        private readonly State.Factory factory;
        private readonly int queriesLeft;
        private readonly Talk talk;
        private readonly Interaction<In, State> interaction;

        public Limited(
            int queriesLeft,
            Talk talk,
            State.Factory factory,
            Interaction<In, State> interaction)
        {
            this.queriesLeft = queriesLeft;
            this.talk = talk;
            this.factory = factory;
            this.interaction = interaction;
        }

        public override async Task<State> RunAsync(In query)
        {
            if (queriesLeft == 0)
            {
                await talk.SayAsync("На сегодня это все, приходи завтра");
                return factory.Selection(0);
            }

            return await interaction.RunAsync(query);
        }
    }
}