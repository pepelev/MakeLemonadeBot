namespace MakeLemonadeBot
{
    public sealed class TelegramCredentials
    {
        public TelegramCredentials(string token)
        {
            Token = token;
        }

        public string Token { get; }
    }
}