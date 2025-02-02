namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    private sealed partial class Trimmer
    {
        private sealed class State(Trimmer trimmer, ArrayPool<T> pool)
        {
            public Trimmer Trimmer { get; } = trimmer;

            public ArrayPool<T> Pool { get; } = pool;
        }
    }
}
