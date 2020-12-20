using System.Threading.Tasks;

namespace FunBot.Conversation
{
    public sealed class Show : Interaction<None, State>
    {
        private readonly Content.Collection collection;
        private readonly Talk talk;
        private readonly State empty;
        private readonly State shown;

        public Show(Content.Collection collection, Talk talk, State empty, State shown)
        {
            this.collection = collection;
            this.talk = talk;
            this.empty = empty;
            this.shown = shown;
        }

        public override async Task<State> RunAsync(None query)
        {
            if (collection.Empty)
            {
                await talk.SayAsync("В этой категории больше ничего не осталось");
                return empty;
            }

            var content = collection.Pick();
            await talk.SayAsync(content.Print());
            content.MarkShown();
            return shown;
        }
    }
}