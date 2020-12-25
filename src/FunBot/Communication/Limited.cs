using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class Limited<In> : Interaction<In, Conversation>
    {
        private readonly Conversation empty;
        private readonly int queriesLeft;
        private readonly Talk talk;
        private readonly Interaction<In, Conversation> interaction;

        public Limited(
            int queriesLeft,
            Talk talk,
            Conversation empty,
            Interaction<In, Conversation> interaction)
        {
            this.queriesLeft = queriesLeft;
            this.talk = talk;
            this.empty = empty;
            this.interaction = interaction;
        }

        public override async Task<Conversation> RunAsync(In query)
        {
            if (queriesLeft == 0)
            {
                await talk.SayAsync("На сегодня это все, приходи завтра");
                return empty;
            }

            return await interaction.RunAsync(query);
        }
    }
}