using System;

namespace MakeLemonadeBot
{
    public abstract class Clock
    {
        public abstract DateTime Now { get; }
    }
}