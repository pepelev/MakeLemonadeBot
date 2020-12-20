namespace FunBot.Configuration
{
    public sealed class ConstUser : User
    {
        public ConstUser(int dailyBudget)
        {
            DailyBudget = dailyBudget;
        }

        public override int DailyBudget { get; }
    }
}