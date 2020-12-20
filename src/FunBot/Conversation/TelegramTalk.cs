using System.Threading.Tasks;
using Telegram.Bot;

namespace FunBot.Conversation
{
    public sealed class TelegramTalk : Talk
    {
        private readonly long chatId;
        private readonly ITelegramBotClient client;
        private readonly Keyboard keyboard;

        public TelegramTalk(long chatId, Keyboard keyboard, ITelegramBotClient client)
        {
            this.chatId = chatId;
            this.keyboard = keyboard;
            this.client = client;
        }

        public override async Task SayAsync(string phrase)
        {
            await client.SendTextMessageAsync(
                chatId,
                phrase,
                replyMarkup: keyboard.Markup
            );
        }
    }
}