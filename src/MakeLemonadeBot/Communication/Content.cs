namespace MakeLemonadeBot.Communication
{
    public abstract class Content
    {
        public abstract string Print();
        public abstract void MarkShown();

        public abstract class Collection
        {
            public abstract bool Empty { get; }
            public abstract Content Pick();
        }
    }
}