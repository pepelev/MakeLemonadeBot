using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using FunBot.Collections;
using FunBot.Conversation;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace FunBot
{
    public sealed class Telegram2 : UpdateSource
    {
        private readonly string botToken;
        private readonly ITelegramBotClient client;
        private readonly SQLiteConnection connection;
        private readonly Offset offset;
        private readonly State.Collection states;

        public Telegram2(
            string botToken,
            ITelegramBotClient client,
            SQLiteConnection connection,
            Offset offset,
            State.Collection states)
        {
            this.botToken = botToken;
            this.client = client;
            this.connection = connection;
            this.offset = offset;
            this.states = states;
        }

        public override async Task<IReadOnlyList<Update>> UpdatesAsync(CancellationToken token)
        {
            var offsetValue = offset.Get();
            var updates = await client.GetUpdatesAsync(
                allowedUpdates: new[] {UpdateType.Message},
                offset: offsetValue,
                timeout: 45,
                limit: 20,
                cancellationToken: token
            );
            return new ProjectingReadOnlyList<Telegram.Bot.Types.Update, Update>(
                updates,
                update => new TelegramUpdate(
                    new TelegramMessage(update),
                    connection,
                    states,
                    botToken
                )
            );
        }
    }
}