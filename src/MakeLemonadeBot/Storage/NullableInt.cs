namespace MakeLemonadeBot.Storage
{
    public sealed class NullableInt : Value
    {
        public NullableInt(int? content)
        {
            Content = content;
        }

        public override object? Content { get; }
    }
}