using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class Limited<In> : Interaction<In, Conversation>
    {
        private readonly Conversation.Factory factory;
        private readonly int queriesLeft;
        private readonly Talk talk;
        private readonly Interaction<In, Conversation> interaction;

        public Limited(
            int queriesLeft,
            Talk talk,
            Conversation.Factory factory,
            Interaction<In, Conversation> interaction)
        {
            this.queriesLeft = queriesLeft;
            this.talk = talk;
            this.factory = factory;
            this.interaction = interaction;
        }

        public override async Task<Conversation> RunAsync(In query)
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