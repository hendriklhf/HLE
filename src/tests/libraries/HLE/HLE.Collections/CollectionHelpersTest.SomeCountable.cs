using System.Collections;
using System.Collections.Generic;

namespace HLE.Collections.UnitTests;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeCountable : IEnumerable<int>, ICountable
    {
        public int Count => 16;

        public IEnumerator<int> GetEnumerator() => null!;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
