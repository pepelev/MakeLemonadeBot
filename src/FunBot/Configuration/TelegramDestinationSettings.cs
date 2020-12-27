namespace FunBot.Configuration
{
    public abstract class TelegramDestinationSettings
    {
        public abstract string Token { get; }
        public abstract string ChatId { get; }
    }
}