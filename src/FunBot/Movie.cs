namespace FunBot
{
    public sealed class Movie
    {
        public Movie(string id, string name, string englishName, int? year)
        {
            Id = id;
            Name = name;
            OriginalName = englishName;
            Year = year;
        }

        public string Id { get; }
        public string Name { get; }
        public string OriginalName { get; }
        public int? Year { get; }
    }
}
