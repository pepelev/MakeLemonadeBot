using FunBot.Sheets;

namespace FunBot.Tests.Complex
{
    public sealed class ConstSheets : Sheet.Collection
    {
        public ConstSheets(Sheet movies, Sheet serials, Sheet books, Sheet cartoons)
        {
            Movies = movies;
            Serials = serials;
            Books = books;
            Cartoons = cartoons;
        }

        public override Sheet Movies { get; }
        public override Sheet Serials { get; }
        public override Sheet Books { get; }
        public override Sheet Cartoons { get; }
    }
}