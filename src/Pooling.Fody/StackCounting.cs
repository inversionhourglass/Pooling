using Fody;
using System.Collections.Generic;

namespace Pooling.Fody
{
    internal class StackCounting
    {
        private readonly List<Item> _items = [];

        public bool Any() => _items.Count > 0;

        public void Add(PoolItem poolItem)
        {
            _items.Add(new(poolItem, 1));
        }

        public void Increase(int count = 1)
        {
            foreach (var item in _items)
            {
                item.StackDepth += count;
            }
        }

        public PoolItem? Decrease(int count = 1)
        {
            var targets = new List<Item>();
            foreach (var item in _items)
            {
                item.StackDepth -= count;
                if (item.StackDepth <= 0)
                {
                    targets.Add(item);
                }
            }

            if (targets.Count == 0) return null;
            if (targets.Count > 1) throw new FodyWeavingException("Failed analysis the instructions, get more than one target at single instruction");

            var target = targets[0];
            _items.Remove(target);

            return target.PoolItem;
        }

        public class Item(PoolItem poolItem, int stackDepth)
        {
            public PoolItem PoolItem { get; } = poolItem;

            public int StackDepth { get; set; } = stackDepth;
        }
    }
}
