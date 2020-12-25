using System.Threading;
using System.Threading.Tasks;
using FunBot.Communication;
using FunBot.Jobs;

namespace FunBot
{
    public sealed class Listening : Job
    {
        private readonly CancellationToken cancellationToken;
        private readonly Conversation.Collection states;
        private readonly UpdateSource updateSource;

        public Listening(
            Conversation.Collection states,
            CancellationToken cancellationToken,
            UpdateSource updateSource)
        {
            this.states = states;
            this.cancellationToken = cancellationToken;
            this.updateSource = updateSource;
        }

        public override async Task RunAsync()
        {
            foreach (var update in await updateSource.UpdatesAsync(cancellationToken))
            {
                await update.Subject.AnswerAsync(update.Text);
            }

            foreach (var (chatId, state) in states.Questions())
            {
                var expiredState = await state.AskAsync();
                states.Update(chatId, expiredState);
            }
        }
    }
}