using System;
using System.Globalization;

namespace FunBot.Storage
{
    public sealed class Instant : Value
    {
        private readonly DateTime value;

        public Instant(DateTime value)
        {
            this.value = value;
        }

        public override object? Content => value.ToString("O", CultureInfo.InvariantCulture);
    }
}