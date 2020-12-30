using System.Data.SQLite;
using Telegram.Bot;

namespace FunBot.Communication
{
    public sealed class TelegramTalks : Talk.Collection
    {
        private readonly ITelegramBotClient client;
        private readonly SQLiteConnection connection;
        private readonly Clock clock;

        public TelegramTalks(ITelegramBotClient client, SQLiteConnection connection, Clock clock)
        {
            this.client = client;
            this.connection = connection;
            this.clock = clock;
        }

        public override Talk For(long chatId, Keyboard keyboard) => new TelegramTalk(
            chatId,
            keyboard,
            client,
            connection,
            clock
        );
    }
}