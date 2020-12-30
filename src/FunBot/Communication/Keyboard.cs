using Telegram.Bot.Types.ReplyMarkups;

namespace MakeLemonadeBot.Communication
{
    public abstract class Keyboard
    {
        public abstract IReplyMarkup Markup { get; }
    }
}