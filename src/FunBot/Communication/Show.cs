using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class Show : Interaction<None, Conversation>
    {
        private readonly Content.Collection collection;
        private readonly Talk talk;
        private readonly Conversation empty;
        private readonly Conversation shown;

        public Show(Content.Collection collection, Talk talk, Conversation empty, Conversation shown)
        {
            this.collection = collection;
            this.talk = talk;
            this.empty = empty;
            this.shown = shown;
        }

        public override async Task<Conversation> RunAsync(None query)
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