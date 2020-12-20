using System;

namespace FunBot
{
    public sealed class Book : IEquatable<Book>
    {
        public Book(string id, string name, string author)
        {
            Id = id;
            Name = name;
            Author = author;
        }

        public string Id { get; }
        public string Name { get; }
        public string Author { get; }
        public string Print() => $"{Author} - {Name}";

        public bool Equals(Book? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Name == other.Name && Author == other.Author;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Book other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Author.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Book? left, Book? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Book? left, Book? right)
        {
            return !Equals(left, right);
        }
    }
}