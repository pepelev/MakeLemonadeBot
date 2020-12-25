using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class FeedbackDialogue : Interaction<None, State>
    {
        private readonly Talk talk;
        private readonly State state;

        public FeedbackDialogue(Talk talk, State state)
        {
            this.talk = talk;
            this.state = state;
        }

        public override async Task<State> RunAsync(None query)
        {
            await talk.SayAsync("Расскажи мне, что ты обо мне думаешь");
            return state;
        }
    }
}