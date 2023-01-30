using System;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using MakeLemonadeBot.Storage;
using Serilog;
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
        private readonly ILogger log;

        public TelegramTalk(
            long chatId,
            Keyboard keyboard,
            ITelegramBotClient client,
            SQLiteConnection connection,
            Clock clock,
            ILogger log)
        {
            this.chatId = chatId;
            this.keyboard = keyboard;
            this.client = client;
            this.connection = connection;
            this.clock = clock;
            this.log = log;
        }

        public override async Task SayAsync(string phrase)
        {
            try
            {
                await client.SendTextMessageAsync(
                    chatId,
                    phrase,
                    replyMarkup: keyboard.Markup
                );
            }
            catch (HttpRequestException e) when (e.Message.StartsWith("Forbidden"))
            {
                log.Warning(e, "Telegram api returned Forbidden");
            }
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