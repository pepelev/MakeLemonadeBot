namespace MakeLemonadeBot.Storage
{
    public sealed class Int : Value
    {
        public Int(int content)
        {
            Content = content;
        }

        public override object? Content { get; }
    }
}