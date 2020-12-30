using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MakeLemonadeBot.Collections
{
    public sealed class FullJoin<Key, Left, Right> : IEnumerable<(Key Key, Left Left, Right Right)>
    {
        private readonly IEnumerable<(Key Id, Left Item)> left;
        private readonly IEnumerable<(Key Id, Right Item)> right;

        public FullJoin(
            IEnumerable<(Key Id, Left Item)> left,
            IEnumerable<(Key Id, Right Item)> right)
        {
            this.left = left;
            this.right = right;
        }

        public IEnumerator<(Key Key, Left Left, Right Right)> GetEnumerator()
        {
            var leftLookup = left.ToLookup(pair => pair.Id, pair => pair.Item);
            var rightLookup = right.ToLookup(pair => pair.Id, pair => pair.Item);
            var keys = leftLookup.Keys().Union(rightLookup.Keys());
            return (
                from key in keys
                from triples in For(key)
                select triples
            ).GetEnumerator();
            
            IEnumerable<(Key Key, Left Left, Right Right)> For(Key key)
            {
                if (!leftLookup.Contains(key))
                {
                    return rightLookup[key].Select(rightItem => (key, default(Left), rightItem));
                }
                if (!rightLookup.Contains(key))
                {
                    return leftLookup[key].Select(leftItem => (key, leftItem, default(Right)));
                }
                return
                    from leftItem in leftLookup[key]
                    from rightItem in rightLookup[key]
                    select (key, leftItem, rightItem);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}