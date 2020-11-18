using System;
using System.Collections.Generic;
using System.Linq;

namespace FunBot
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(Key Id, T Item)> By<Key, T>(this IEnumerable<T> sequence, Func<T, Key> id) =>
            sequence.Select(item => (id(item), item));

        public static IEnumerable<Key> Keys<Key, Element>(this IEnumerable<IGrouping<Key, Element>> groups) =>
            groups.Select(group => group.Key);
    }
}