using System;
using System.Collections.Generic;

namespace MakeLemonadeBot.Collections
{
    public static class Full
    {
        public static IEnumerable<(Key Key, Left Left, Right Right)> Join<Key, Left, Right>(
            IEnumerable<(Key Id, Left Item)> left,
            IEnumerable<(Key Id, Right Item)> right) => new FullJoin<Key, Left, Right>(left, right);

        public static IEnumerable<(Key Key, T Left, T Right)> Join<Key, T>(
            IEnumerable<T> left,
            IEnumerable<T> right,
            Func<T, Key> key) => new FullJoin<Key, T, T>(left.By(key), right.By(key));
    }
}