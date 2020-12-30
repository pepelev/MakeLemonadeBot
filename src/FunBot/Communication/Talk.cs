using System.Threading.Tasks;

namespace MakeLemonadeBot.Communication
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