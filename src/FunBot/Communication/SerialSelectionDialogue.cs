using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class SerialSelectionDialogue : Interaction<None, Conversation>
    {
        private readonly Talk talk;
        private readonly Conversation choice;

        public SerialSelectionDialogue(Talk talk, Conversation choice)
        {
            this.talk = talk;
            this.choice = choice;
        }

        public override async Task<Conversation> RunAsync(None query)
        {
            await talk.SayAsync("Какой, длинный или короткий?");
            return choice;
        }
    }
}