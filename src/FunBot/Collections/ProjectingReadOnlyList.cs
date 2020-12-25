using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FunBot.Collections
{
    internal sealed class ProjectingReadOnlyList<T, TResult> : IReadOnlyList<TResult>
    {
        private readonly IList<T> list;
        private readonly Func<T, TResult> projection;

        public ProjectingReadOnlyList(IList<T> list, Func<T, TResult> projection)
        {
            this.list = list;
            this.projection = projection;
        }

        public IEnumerator<TResult> GetEnumerator() => list.Select(projection).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => list.Count;
        public TResult this[int index] => projection(list[index]);
    }
}