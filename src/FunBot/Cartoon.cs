using System;
using System.Globalization;

namespace FunBot
{
    public sealed class Cartoon : IEquatable<Cartoon>
    {
        public Cartoon(string id, string name, string originalName, int year, string? note)
        {
            Id = id;
            Name = name;
            OriginalName = originalName;
            Year = year;
            Note = note;
        }

        public string Id { get; }
        public string Name { get; }
        public string OriginalName { get; }
        public int Year { get; }
        public string? Note { get; }

        public string Print() => (StringComparer.InvariantCulture.Equals(Name, OriginalName), Note) switch
        {
            (true, null) => $"{Name}, {Year}",
            (false, null) => $"{Name} ({OriginalName}), {Year}",
            (true, { } note) => $"{Name}, {Year} - {note}",
            (false, { } note) => $"{Name} ({OriginalName}), {Year} - {note}"
        };

        public override string ToString() => Print();

        public bool Equals(Cartoon? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Id == other.Id &&
                Name == other.Name &&
                OriginalName == other.OriginalName &&
                Year == other.Year &&
                Note == other.Note;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Cartoon other && Equals(other);
        }

        public static bool operator ==(Cartoon? left, Cartoon? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Cartoon? left, Cartoon? right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ OriginalName.GetHashCode();
                hashCode = (hashCode * 397) ^ Year;
                hashCode = (hashCode * 397) ^ (Note?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}