using System.Data.SQLite;
using System.Threading.Tasks;
using MakeLemonadeBot.Storage;
using Telegram.Bot;

namespace MakeLemonadeBot.Communication
{
    public sealed class TelegramTalk : Talk
    {
        private readonly long chatId;
        private readonly ITelegramBotClient client;
        private readonly Clock clock;
        private readonly SQLiteConnection connection;
        private readonly Keyboard keyboard;

        public TelegramTalk(
            long chatId,
            Keyboard keyboard,
            ITelegramBotClient client,
            SQLiteConnection connection,
            Clock clock)
        {
            this.chatId = chatId;
            this.keyboard = keyboard;
            this.client = client;
            this.connection = connection;
            this.clock = clock;
        }

        public override async Task SayAsync(string phrase)
        {
            await client.SendTextMessageAsync(
                chatId,
                phrase,
                replyMarkup: keyboard.Markup
            );
            connection.Execute(
                @"INSERT INTO messages (id, chat_id, date, text, author)
                VALUES (-1, :chat_id, :date, :text, '@Bot')",
                ("chat_id", chatId),
                ("date", clock.Now.ToString("O")),
                ("text", phrase)
            );
        }
    }
}