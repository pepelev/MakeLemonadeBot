using Telegram.Bot.Types.ReplyMarkups;

namespace FunBot.Communication
{
    public abstract class Keyboard
    {
        public abstract IReplyMarkup Markup { get; }
    }
}