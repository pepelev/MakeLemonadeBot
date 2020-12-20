using System;

namespace FunBot
{
    public sealed class Serial : IEquatable<Serial>
    {
        public Serial(
            string id,
            string name,
            string originalName,
            int year,
            SerialDuration duration)
        {
            Id = id;
            Name = name;
            OriginalName = originalName;
            Year = year;
            Duration = duration;
        }

        public string Id { get; }
        public string Name { get; }
        public string OriginalName { get; }
        public int Year { get; }
        public SerialDuration Duration { get; }
        public string Print() => $"{Name} ({OriginalName}), {Year}";

        public bool Equals(Serial? other)
        {
            if (ReferenceEquals(null, other)) 
                return false;

            if (ReferenceEquals(this, other)) 
                return true;

            return Id == other.Id && 
                   Name == other.Name &&
                   OriginalName == other.OriginalName &&
                   Year == other.Year &&
                   Duration == other.Duration;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Serial other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ OriginalName.GetHashCode();
                hashCode = (hashCode * 397) ^ Year;
                hashCode = (hashCode * 397) ^ (int) Duration;
                return hashCode;
            }
        }

        public static bool operator ==(Serial? left, Serial? right) => Equals(left, right);
        public static bool operator !=(Serial? left, Serial? right) => !Equals(left, right);
    }
}