namespace FunBot
{
    public abstract class Aspect<T>
    {
        public abstract bool Changed(T old, T @new);
        public abstract string Print(T old, T @new);
    }
}