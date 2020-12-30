namespace MakeLemonadeBot.Configuration
{
    public abstract class TelegramSettings
    {
        public abstract TelegramListeningSettings Listening { get; }
        public abstract TelegramDestinationSettings Feedback { get; }
        public abstract TelegramDestinationSettings Log { get; }
    }
}