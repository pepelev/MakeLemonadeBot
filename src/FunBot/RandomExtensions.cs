using System;
using System.Collections.Generic;

namespace FunBot
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