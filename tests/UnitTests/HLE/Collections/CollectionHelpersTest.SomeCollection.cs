using System;
using System.Collections;
using System.Collections.Generic;
using HLE.Collections;
using HLE.Memory;

namespace HLE.UnitTests.Collections;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeCollection : ICollection<int>
    {
        public int Count => _items.Length;

        public bool IsReadOnly => false;

        private readonly int[] _items;

        public SomeCollection(int length)
        {
            int[] items = new int[length];
            Random.Shared.Fill(items);
            _items = items;
        }

        public Span<int> AsSpan() => _items;

        public void Add(int item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(int item) => throw new NotSupportedException();

        public void CopyTo(int[] array, int arrayIndex)
        {
            CopyWorker<int> copyWorker = new(_items);
            copyWorker.CopyTo(array, arrayIndex);
        }

        public bool Remove(int item) => throw new NotSupportedException();

        public IEnumerator<int> GetEnumerator() => new ArrayEnumerator<int>(_items);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
