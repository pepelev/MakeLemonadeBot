using System;
using System.Linq;

namespace FunBot
{
    public sealed class Thing : IEquatable<Thing>
    {
        public Thing(string id, string content, string? description, string? category)
        {
            Id = id;
            Content = content;
            Description = description;
            Category = category;
        }

        public string Id { get; }
        public string Content { get; }
        public string? Description { get; }
        public string? Category { get; }

        public string Print() => string.Join(
            "\n\n",
            new[] {Content, Description}.Where(@string => !string.IsNullOrEmpty(@string))
        );

        public override string ToString() => Print();

        public bool Equals(Thing? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Id == other.Id &&
                Content == other.Content &&
                Description == other.Description &&
                Category == other.Category;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Thing other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Content.GetHashCode();
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Category?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(Thing? left, Thing? right) => Equals(left, right);
        public static bool operator !=(Thing? left, Thing? right) => !Equals(left, right);
    }
}