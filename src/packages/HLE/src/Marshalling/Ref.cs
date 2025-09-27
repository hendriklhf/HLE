#if !NET9_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#endif

namespace HLE.Marshalling;

#if NET9_0_OR_GREATER
internal readonly ref struct Ref<T>(ref T value)
{
    public ref T Value => ref _value;

    private readonly ref T _value = ref value;
}
#else
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
[SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
internal readonly unsafe struct Ref<T>(ref T value)
{
    public ref T Value => ref Unsafe.AsRef<T>(_value);

    private readonly T* _value = (T*)Unsafe.AsPointer(ref value);
}
#endif
