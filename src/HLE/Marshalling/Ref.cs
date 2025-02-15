namespace HLE.Marshalling;

internal readonly ref struct Ref<T>(ref T value)
{
    public ref T Value => ref _value;

    private readonly ref T _value = ref value;
}
