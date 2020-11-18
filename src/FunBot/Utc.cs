using System;

namespace FunBot
{
    public sealed class Utc : Clock
    {
        public override DateTime Now => DateTime.UtcNow;
    }
}