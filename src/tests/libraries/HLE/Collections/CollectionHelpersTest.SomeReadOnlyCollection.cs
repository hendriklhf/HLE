using System.Collections;
using System.Collections.Generic;

namespace HLE.UnitTests.Collections;

public sealed partial class CollectionHelpersTest
{
    private sealed class SomeReadOnlyCollection : IReadOnlyCollection<int>
    {
        public int Count => 16;

        public IEnumerator<int> GetEnumerator() => null!;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
