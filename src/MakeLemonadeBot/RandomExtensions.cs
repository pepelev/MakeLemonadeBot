using System;
using System.Collections.Generic;

namespace MakeLemonadeBot
{
    public static class RandomExtensions
    {
        public static T Pick<T>(this Random random, IReadOnlyList<T> list)
        {
            var index = random.Next(list.Count);
            return list[index];
        }
    }
}