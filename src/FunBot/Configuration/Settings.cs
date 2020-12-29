namespace FunBot.Configuration
{
    public abstract class Settings
    {
        public abstract TelegramSettings Telegram { get; }
        public abstract Source.Collection Sources { get; }
        public abstract User.Collection Users { get; }
        public abstract Phrases Phrases { get; }
    }
}