using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class SerialSelectionDialogue : Interaction<None, State>
    {
        private readonly State.Factory factory;
        private readonly Talk talk;
        private readonly int queriesLeft;

        public SerialSelectionDialogue(State.Factory factory, Talk talk, int queriesLeft)
        {
            this.factory = factory;
            this.talk = talk;
            this.queriesLeft = queriesLeft;
        }

        public override async Task<State> RunAsync(None query)
        {
            await talk.SayAsync("Какой, длинный или короткий?");
            return factory.SerialSelection(queriesLeft);
        }
    }
}