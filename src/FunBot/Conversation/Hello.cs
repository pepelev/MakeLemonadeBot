using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace FunBot.Conversation
{
    public sealed class Hello : State
    {
        private readonly ITelegramBotClient client;
        private readonly long chatId;

        public Hello(ITelegramBotClient client, long chatId)
        {
            this.client = client;
            this.chatId = chatId;
        }

        public override async Task<State> RespondAsync(string query)
        {
            await client.SendTextMessageAsync(
                chatId,
                "Привет, это отличный бот",
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
            return new Selection(client, chatId, 5);
        }

        public override Task<State> ExpireAsync() => Task.FromResult<State>(this);
        public override DateTime ExpiresAt => Expires.Never;
        public override byte[] Serialize() => new byte[] {0x01};
    }
}