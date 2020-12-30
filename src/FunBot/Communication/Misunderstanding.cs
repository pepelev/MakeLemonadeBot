using System.Threading.Tasks;

namespace MakeLemonadeBot.Communication
{
    public sealed class Misunderstanding : Interaction<None, Conversation>
    {
        private readonly Talk talk;
        private readonly Conversation next;

        public Misunderstanding(Talk talk, Conversation next)
        {
            this.talk = talk;
            this.next = next;
        }

        public override async Task<Conversation> RunAsync(None query)
        {
            await talk.SayAsync("Я не понял тебя");
            return next;
        }
    }
}