using System;

namespace FunBot
{
    public abstract class Clock
    {
        public abstract DateTime Now { get; }
    }
}