using System;
using System.Collections;
using System.Collections.Generic;
using HLE.Collections;

namespace HLE.UnitTests.Collections;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeIndexable : IEnumerable<int>, IIndexable<int>
    {
        public int this[int index] => _items[index];

        public int this[Index index] => _items[index];

        public int Count => _items.Length;

        private readonly int[] _items;

        public SomeIndexable(int length)
        {
            int[] items = new int[length];
            Random.Shared.Fill(items);
            _items = items;
        }

        public Span<int> AsSpan() => _items;

        public IEnumerator<int> GetEnumerator() => new ArrayEnumerator<int>(_items);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
