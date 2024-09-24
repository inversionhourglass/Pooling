using Fody;
using System.Collections.Generic;

namespace Pooling.Fody
{
    internal class StackCounting
    {
        private readonly List<Item> _items = [];
        private int _stackDepth;

        public bool Any() => _items.Count > 0;

        public void Add(PoolItem poolItem)
        {
            _items.Add(new(poolItem, _stackDepth));
        }

        public void Increase(int count = 1)
        {
            _stackDepth += count;
            foreach (var item in _items)
            {
                item.StackDepth += count;
            }
        }

        public PoolItem? Decrease(int count = 1)
        {
            _stackDepth -= count;
            if (_stackDepth < 0) throw new FodyWeavingException("Failed analysis the instructions, get negative value of global stack depth.");

            var targets = new List<Item>();
            foreach (var item in _items)
            {
                item.StackDepth -= count;
                if (item.StackDepth == 0)
                {
                    targets.Add(item);
                }
                else if (item.StackDepth < 0)
                {
                    throw new FodyWeavingException("Failed analysis the instructions, get negative value of stack depth.");
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
