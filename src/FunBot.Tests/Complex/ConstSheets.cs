using FunBot.Sheets;

namespace FunBot.Tests.Complex
{
    public sealed class ConstSheets : Sheet.Collection
    {
        public ConstSheets(Sheet movies, Sheet serials, Sheet books)
        {
            Movies = movies;
            Serials = serials;
            Books = books;
        }

        public override Sheet Movies { get; }
        public override Sheet Serials { get; }
        public override Sheet Books { get; }
    }
}