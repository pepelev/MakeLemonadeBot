using System;

namespace FunBot
{
    public sealed class Movie : IEquatable<Movie>
    {
        public Movie(string id, string name, string? originalName, int? year)
        {
            Id = id;
            Name = name;
            OriginalName = originalName;
            Year = year;
        }

        public string Id { get; }
        public string Name { get; }
        public string? OriginalName { get; }
        public int? Year { get; }

        public bool Equals(Movie? other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id == other.Id &&
                   Name == other.Name &&
                   OriginalName == other.OriginalName &&
                   Year == other.Year;
        }

        public string Print() => (OriginalName, Year) switch
        {
            (null, null) => Name,
            (null, { } year) => $"{Name}, {year}",
            ({ } originalName, null) => $"{Name} ({originalName})",
            ({ } originalName, { } year) => $"{Name} ({originalName}), {year}"
        };

        public override string ToString() => Name;

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Movie other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (OriginalName != null ? OriginalName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Year.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Movie? left, Movie? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Movie? left, Movie? right)
        {
            return !Equals(left, right);
        }
    }
}