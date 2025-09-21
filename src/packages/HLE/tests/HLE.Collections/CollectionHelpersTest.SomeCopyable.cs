using System;
using System.Collections;
using System.Collections.Generic;
using HLE.Memory;

namespace HLE.Collections.UnitTests;

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
            => SpanHelpers.CopyChecked(_items, destination, offset);

        public void CopyTo(int[] destination, int offset = 0)
            => SpanHelpers.CopyChecked(_items, destination.AsSpan(offset..));

        public void CopyTo(Memory<int> destination) => SpanHelpers.CopyChecked(_items, destination.Span);

        public void CopyTo(Span<int> destination) => SpanHelpers.CopyChecked(_items, destination);

        public void CopyTo(ref int destination) => SpanHelpers.Copy(_items, ref destination);

        public unsafe void CopyTo(int* destination) => SpanHelpers.Copy(_items, destination);

        public IEnumerator<int> GetEnumerator() => new ArrayEnumerator<int>(_items);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
