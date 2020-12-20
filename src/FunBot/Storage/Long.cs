namespace FunBot.Storage
{
    public sealed class Long : Value
    {
        public Long(long content)
        {
            Content = content;
        }

        public override object? Content { get; }
    }
}