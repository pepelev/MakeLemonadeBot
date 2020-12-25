using System.Threading.Tasks;

namespace FunBot.Communication
{
    public sealed class FeedbackDialogue : Interaction<None, Conversation>
    {
        private readonly Talk talk;
        private readonly Conversation conversation;

        public FeedbackDialogue(Talk talk, Conversation conversation)
        {
            this.talk = talk;
            this.conversation = conversation;
        }

        public override async Task<Conversation> RunAsync(None query)
        {
            await talk.SayAsync("Расскажи мне, что ты обо мне думаешь");
            return conversation;
        }
    }
}