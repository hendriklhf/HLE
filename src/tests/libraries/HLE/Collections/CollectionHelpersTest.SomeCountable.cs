using System.Collections;
using System.Collections.Generic;
using HLE.Collections;

namespace HLE.UnitTests.Collections;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeCountable : IEnumerable<int>, ICountable
    {
        public int Count => 16;

        public IEnumerator<int> GetEnumerator() => null!;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
