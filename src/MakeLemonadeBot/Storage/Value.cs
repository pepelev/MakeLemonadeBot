using System;

namespace MakeLemonadeBot.Storage
{
    public abstract class Value
    {
        public abstract object? Content { get; }
        public static implicit operator Value(int @int) => new Int(@int);
        public static implicit operator Value(long @long) => new Long(@long);
        public static implicit operator Value(int? @int) => new NullableInt(@int);
        public static implicit operator Value(string? @string) => new String(@string);
        public static implicit operator Value(DateTime instant) => new Instant(instant);
    }
}