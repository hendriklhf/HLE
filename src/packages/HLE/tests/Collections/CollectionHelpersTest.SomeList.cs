using System;
using System.Collections;
using System.Collections.Generic;
using HLE.Collections;

namespace HLE.UnitTests.Collections;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeList : IList<int>
    {
        private readonly int[] _items;

        public int this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public int Count => _items.Length;

        public bool IsReadOnly => false;

        public SomeList(int length)
        {
            int[] items = new int[length];
            Random.Shared.Fill(items);
            _items = items;
        }

        public void Add(int item) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(int item) => throw new NotSupportedException();

        public void CopyTo(int[] array, int arrayIndex) => throw new NotSupportedException();

        public bool Remove(int item) => throw new NotSupportedException();

        public int IndexOf(int item) => throw new NotSupportedException();

        public void Insert(int index, int item) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

        public IEnumerator<int> GetEnumerator() => new ArrayEnumerator<int>(_items);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
