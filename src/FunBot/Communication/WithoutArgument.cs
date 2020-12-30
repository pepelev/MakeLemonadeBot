using System.Threading.Tasks;

namespace MakeLemonadeBot.Communication
{
    public sealed class WithoutArgument<In, Out> : Interaction<In, Out>
    {
        private readonly Interaction<None, Out> interaction;

        public WithoutArgument(Interaction<None, Out> interaction)
        {
            this.interaction = interaction;
        }

        public override async Task<Out> RunAsync(In query) => await interaction.RunAsync(new None());
    }
}