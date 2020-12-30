namespace MakeLemonadeBot.Sheets
{
    public readonly struct Location
    {
        public static Location NotFound => new Location(-1);

        public int Index { get; }

        public Location(int index)
        {
            Index = index;
        }

        public bool Found => Index >= 0;
    }
}
