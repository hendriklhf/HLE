namespace HLE.Memory;

public sealed partial class ObjectPool<T>
{
    public interface IFactory
    {
        T Create();

        void Return(T obj);
    }
}
