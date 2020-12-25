using System.Threading.Tasks;

namespace FunBot.Communication
{
    public abstract class Interaction<In, Out>
    {
        public abstract Task<Out> RunAsync(In query);
    }
}