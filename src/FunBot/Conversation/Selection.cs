using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace FunBot.Conversation
{
    public sealed class Selection : State
    {
        private readonly ITelegramBotClient client;
        private readonly long chatId;
        private readonly int queriesLeft;

        public Selection(ITelegramBotClient client, long chatId, int queriesLeft)
        {
            this.client = client;
            this.chatId = chatId;
            this.queriesLeft = queriesLeft;
        }

        public override async Task<State> RespondAsync(string query)
        {
            switch (query.ToLowerInvariant())
            {
                case "кино":
                    if (queriesLeft == 0)
                    {
                        await SendAsync("На сегодня это все, приходи завтра");
                        return this;
                    }
                    await SendAsync("Держи кино");
                    return new Selection(client, chatId, queriesLeft - 1);
                case "книги":
                    if (queriesLeft == 0)
                    {
                        await SendAsync("На сегодня это все, приходи завтра");
                        return this;
                    }
                    await SendAsync("Держи книгу");
                    return new Selection(client, chatId, queriesLeft - 1);
                case "кладовая":
                    if (queriesLeft == 0)
                    {
                        await SendAsync("На сегодня это все, приходи завтра");
                        return this;
                    }
                    await SendAsync("Держи кладовку");
                    return new Selection(client, chatId, queriesLeft - 1);
                case "сериалы":
                    if (queriesLeft == 0)
                    {
                        await SendAsync("На сегодня это все, приходи завтра");
                        return this;
                    }

                    await SerialSelectAsync();
                    return new SerialSelection(client, chatId, queriesLeft);
                case "написать нам":
                    await SendAsync("это пока не готово");
                    return this;
                default:
                    await SendAsync("не понял тебя");
                    return this;
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

        private async Task SerialSelectAsync()
        {
            await client.SendTextMessageAsync(
                chatId,
                "Длинные или короткие?",
                replyMarkup: new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton("Длинные"),
                        new KeyboardButton("Короткие")
                    }
                )
            );
        }

        public override Task<State> ExpireAsync() => Task.FromResult<State>(this);
        public override DateTime ExpiresAt => Expires.Never;
        public override byte[] Serialize() => new byte[] { 0x01 };
    }
}