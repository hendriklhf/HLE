using System;
using System.Collections;
using System.Collections.Generic;
using HLE.Collections;
using HLE.Memory;

namespace HLE.UnitTests.Collections;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeCopyable : IEnumerable<int>, ICopyable<int>
    {
        public int Count => _items.Length;

        private readonly int[] _items;

        public SomeCopyable(int length)
        {
            int[] items = new int[length];
            Random.Shared.Fill(items);
            _items = items;
        }

        public Span<int> AsSpan() => _items;

        public void CopyTo(List<int> destination, int offset = 0)
        {
            CopyWorker<int> copyWorker = new(_items);
            copyWorker.CopyTo(destination, offset);
        }

        public void CopyTo(int[] destination, int offset = 0)
        {
            CopyWorker<int> copyWorker = new(_items);
            copyWorker.CopyTo(destination, offset);
        }

        public void CopyTo(Memory<int> destination)
        {
            CopyWorker<int> copyWorker = new(_items);
            copyWorker.CopyTo(destination);
        }

        public void CopyTo(Span<int> destination)
        {
            CopyWorker<int> copyWorker = new(_items);
            copyWorker.CopyTo(destination);
        }

        public void CopyTo(ref int destination)
        {
            CopyWorker<int> copyWorker = new(_items);
            copyWorker.CopyTo(ref destination);
        }

        public unsafe void CopyTo(int* destination)
        {
            CopyWorker<int> copyWorker = new(_items);
            copyWorker.CopyTo(destination);
        }

        public IEnumerator<int> GetEnumerator() => new ArrayEnumerator<int>(_items);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
