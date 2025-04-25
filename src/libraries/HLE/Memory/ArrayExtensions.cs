using System.Collections.Generic;

namespace HLE.Memory;

public static class ArrayExtensions
{
    extension<T>(T[] source)
    {
        public void CopyTo(List<T> destination, int offset = 0)
            => SpanHelpers.CopyChecked(source, destination, offset);

        public void CopyTo(ref T destination)
            => SpanHelpers.Copy(source, ref destination);

        public unsafe void CopyTo(T* destination)
            => SpanHelpers.Copy(source, destination);
    }
}
