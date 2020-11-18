using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace FunBot.Conversation
{
    public sealed class SerialSelection : State
    {
        private readonly ITelegramBotClient client;
        private readonly long chatId;
        private readonly int queriesLeft;

        public SerialSelection(ITelegramBotClient client, long chatId, int queriesLeft)
        {
            this.client = client;
            this.chatId = chatId;
            this.queriesLeft = queriesLeft;
        }

        public override async Task<State> RespondAsync(string query)
        {
            switch (query.ToLowerInvariant())
            {
                case "длинные":
                    await SendAsync("Держи длинный сериал");
                    return new Selection(client, chatId, queriesLeft - 1);
                case "короткие":
                    await SendAsync("Держи короткий сериал");
                    return new Selection(client, chatId, queriesLeft - 1);
                default:
                    await SendAsync("не понял тебя");
                    return new Selection(client, chatId, queriesLeft);
            }
        }

        private async Task SendAsync(string text)
        {
            await client.SendTextMessageAsync(
                chatId,
                text,
                replyMarkup: new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton("Кино"),
                        new KeyboardButton("Сериалы"),
                        new KeyboardButton("Книги"),
                        new KeyboardButton("Кладовая"),
                        new KeyboardButton("Написать нам")
                    }
                )
            );
        }

        public override Task<State> ExpireAsync() => Task.FromResult<State>(this);
        public override DateTime ExpiresAt => Expires.Never;
        public override byte[] Serialize() => new byte[] { 0x01 };
    }
}