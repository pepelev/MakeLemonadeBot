using System.Data.SQLite;
using Serilog;
using Telegram.Bot;

namespace MakeLemonadeBot.Communication
{
    public sealed class TelegramTalks : Talk.Collection
    {
        private readonly ITelegramBotClient client;
        private readonly SQLiteConnection connection;
        private readonly Clock clock;
        private readonly ILogger log;

        public TelegramTalks(ITelegramBotClient client, SQLiteConnection connection, Clock clock, ILogger log)
        {
            this.client = client;
            this.connection = connection;
            this.clock = clock;
            this.log = log;
        }

        public override Talk For(long chatId, Keyboard keyboard) => new TelegramTalk(
            chatId,
            keyboard,
            client,
            connection,
            clock,
            log
        );
    }
}