namespace FunBot
{
    public static class StringExtensions
    {
        public static int? AsNumber(this string text)
        {
            if (int.TryParse(text, out var result))
            {
                return result;
            }

            return null;
        }
    }
}