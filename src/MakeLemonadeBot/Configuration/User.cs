namespace MakeLemonadeBot.Configuration
{
    public abstract class User
    {
        public abstract int DailyBudget { get; }

        public abstract class Collection
        {
            public abstract User Get(long chatId);
        }
    }
}