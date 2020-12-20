namespace FunBot.Commands
{
    public abstract class Command<T>
    {
        public abstract T Run();
    }
}