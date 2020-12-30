using System.Collections.Generic;
using System.Threading.Tasks;

namespace MakeLemonadeBot.Communication
{
    public sealed class Matching<In, Out> : Interaction<In, Out>
    {
        private readonly In expectation;
        private readonly IEqualityComparer<In> equality;
        private readonly Interaction<In, Out> match;
        private readonly Interaction<In, Out> rest;

        public Matching(
            In expectation,
            IEqualityComparer<In> equality,
            Interaction<In, Out> match,
            Interaction<In, Out> rest)
        {
            this.expectation = expectation;
            this.equality = equality;
            this.match = match;
            this.rest = rest;
        }

        public override async Task<Out> RunAsync(In query)
        {
            var interaction = equality.Equals(expectation, query)
                ? match
                : rest;

            return await interaction.RunAsync(query);
        }
    }
}