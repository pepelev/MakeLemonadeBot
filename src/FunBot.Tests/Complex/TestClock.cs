using System;

namespace FunBot.Tests.Complex
{
    public sealed class TestClock : Clock
    {
        public TestClock(DateTime now)
        {
            this.now = now;
        }

        private DateTime now;
        public override DateTime Now => now;

        public void Pass(DateTime newDate)
        {
            if (Now >= newDate)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(newDate),
                    newDate,
                    $"must be greater than Now = {Now}"
                );
            }

            now = newDate;
        }
    }
}