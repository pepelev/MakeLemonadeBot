using Telegram.Bot;

namespace FunBot.Communication
{
    public sealed class TelegramTalks : Talk.Collection
    {
        private readonly ITelegramBotClient client;

        public TelegramTalks(ITelegramBotClient client)
        {
            this.client = client;
        }

        public override Talk For(long chatId, Keyboard keyboard) => new TelegramTalk(chatId, keyboard, client);
    }
}