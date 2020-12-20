using Telegram.Bot.Types.ReplyMarkups;

namespace FunBot.Conversation
{
    public abstract class Keyboard
    {
        public abstract IReplyMarkup Markup { get; }
    }
}