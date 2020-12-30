using MakeLemonadeBot.Sheets;

namespace MakeLemonadeBot.Tests.Complex
{
    public sealed class ConstSheets : Sheet.Collection
    {
        public ConstSheets(Sheet movies, Sheet serials, Sheet books, Sheet cartoons, Sheet storeroom)
        {
            Movies = movies;
            Serials = serials;
            Books = books;
            Cartoons = cartoons;
            Storeroom = storeroom;
        }

        public override Sheet Movies { get; }
        public override Sheet Serials { get; }
        public override Sheet Books { get; }
        public override Sheet Cartoons { get; }
        public override Sheet Storeroom { get; }
    }
}