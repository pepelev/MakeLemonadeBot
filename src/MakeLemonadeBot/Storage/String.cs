namespace MakeLemonadeBot.Storage
{
    public sealed class String : Value
    {
        public String(string? content)
        {
            Content = content;
        }

        public override object? Content { get; }
    }
}