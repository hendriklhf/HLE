using System;
using System.Collections;
using System.Collections.Generic;
using HLE.Collections;

namespace HLE.UnitTests.Collections;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeReadOnlyList : IReadOnlyList<int>
    {
        public int this[int index] => _items[index];

        public int Count => _items.Length;

        private readonly int[] _items;

        public SomeReadOnlyList(int length)
        {
            int[] items = new int[length];
            Random.Shared.Fill(items);
            _items = items;
        }

        public IEnumerator<int> GetEnumerator() => new ArrayEnumerator<int>(_items);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
