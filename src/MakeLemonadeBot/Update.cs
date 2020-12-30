using MakeLemonadeBot.Communication;

namespace MakeLemonadeBot
{
    public abstract class Update
    {
        public abstract string Text { get; }
        public abstract Conversation Subject { get; }
    }
}