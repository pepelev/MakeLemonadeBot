using System;

namespace MakeLemonadeBot
{
    public sealed class Utc : Clock
    {
        public override DateTime Now => DateTime.UtcNow;
    }
}