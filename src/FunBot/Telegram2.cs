using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MakeLemonadeBot.Collections;
using MakeLemonadeBot.Communication;
using MakeLemonadeBot.Storage;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MakeLemonadeBot
{
    public sealed class Telegram2 : UpdateSource
    {
        private readonly string botToken;
        private readonly ITelegramBotClient client;
        private readonly SQLiteConnection connection;
        private readonly Offset offset;
        private readonly Conversation.Collection states;

        public Telegram2(
            string botToken,
            ITelegramBotClient client,
            SQLiteConnection connection,
            Offset offset,
            Conversation.Collection states)
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

            var textUpdates = updates
                .Where(update => !string.IsNullOrWhiteSpace(update.Message.Text))
                .ToList();

            StoreMessages(textUpdates);

            return new ProjectingReadOnlyList<Telegram.Bot.Types.Update, Update>(
                textUpdates,
                update => new TelegramUpdate(
                    new TelegramMessage(update),
                    connection,
                    states,
                    botToken
                )
            );
        }

        private void StoreMessages(IEnumerable<Telegram.Bot.Types.Update> textUpdates)
        {
            using var transaction = connection.BeginTransaction();
            foreach (var update in textUpdates)
            {
                var message = update.Message;
                var chat = message.Chat;
                transaction.Execute(
                    @"INSERT INTO messages (id, chat_id, date, text, author)
                        VALUES (:id, :chat_id, :date, :text, :author)",
                    ("id", update.Id),
                    ("chat_id", chat.Id),
                    ("date", message.Date.ToString("O")),
                    ("text", message.Text),
                    ("author", $"{chat.FirstName} {chat.LastName} {chat.Description}")
                );
            }

            transaction.Commit();
        }
    }
}