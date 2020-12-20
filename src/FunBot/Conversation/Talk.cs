using System.Threading.Tasks;

namespace FunBot.Conversation
{
    public abstract class Talk
    {
        public abstract Task SayAsync(string phrase);

        public abstract class Collection
        {
            public abstract Talk For(long chatId, Keyboard keyboard);
        }
    }
}