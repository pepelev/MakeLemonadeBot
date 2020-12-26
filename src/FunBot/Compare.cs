using System.Collections.Generic;

namespace FunBot
{
    public static class Compare
    {
        public static T Max<T>(T a, T b) => Comparer<T>.Default.Max(a, b);
        public static T Min<T>(T a, T b) => Comparer<T>.Default.Min(a, b);

        public static T Max<T>(this IComparer<T> order, T a, T b) => order.Compare(a, b) > 0
            ? a
            : b;

        public static T Min<T>(this IComparer<T> order, T a, T b) => order.Compare(a, b) < 0
            ? a
            : b;
    }
}