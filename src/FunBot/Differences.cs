using System;
using System.Collections;
using System.Collections.Generic;

namespace FunBot
{
    public sealed class Differences<Item, Key> : IEnumerable<string>
    {
        private readonly Aspect<Item>[] aspects;
        private readonly Func<Item, Key> key;
        private readonly IEnumerable<Item> left;
        private readonly IEnumerable<Item> right;

        public Differences(
            IEnumerable<Item> left,
            IEnumerable<Item> right,
            Func<Item, Key> key,
            Aspect<Item>[] aspects)
        {
            this.left = left;
            this.right = right;
            this.key = key;
            this.aspects = aspects;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var join = Full.Join(left, right, key);
            foreach (var (joinKey, @new, old) in join)
            {
                if (@new == null)
                {
                    yield return $"{joinKey}: Removed {old}";
                }
                else if (old == null)
                {
                    yield return $"{joinKey}: Added {@new}";
                }
                else
                {
                    foreach (var aspect in aspects)
                    {
                        if (aspect.Changed(old, @new))
                        {
                            yield return $"{joinKey}: {aspect.Print(old, @new)}";
                        }
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}