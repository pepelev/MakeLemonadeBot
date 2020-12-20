using System.Threading.Tasks;

namespace FunBot.Conversation
{
    public sealed class Misunderstanding : Interaction<None, State>
    {
        private readonly Talk talk;
        private readonly State next;

        public Misunderstanding(Talk talk, State next)
        {
            this.talk = talk;
            this.next = next;
        }

        public override async Task<State> RunAsync(None query)
        {
            await talk.SayAsync("Я не понял тебя");
            return next;
        }
    }
}