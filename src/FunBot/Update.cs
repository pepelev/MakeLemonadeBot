using FunBot.Communication;

namespace FunBot
{
    public abstract class Update
    {
        public abstract string Text { get; }
        public abstract State Subject { get; }
    }
}