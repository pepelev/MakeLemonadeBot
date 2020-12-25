using System.Threading;
using System.Threading.Tasks;
using FunBot.Conversation;
using FunBot.Jobs;

namespace FunBot
{
    public sealed class Listening : Job
    {
        private readonly CancellationToken cancellationToken;
        private readonly State.Collection states;
        private readonly UpdateSource updateSource;

        public Listening(
            State.Collection states,
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
                await update.Subject.RespondAsync(update.Text);
            }

            foreach (var (chatId, state) in states.Expired())
            {
                var expiredState = await state.ExpireAsync();
                states.Update(chatId, expiredState);
            }
        }
    }
}