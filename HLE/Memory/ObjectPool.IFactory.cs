namespace HLE.Memory;

public sealed partial class ObjectPool<T>
{
    public interface IFactory
    {
        /// <summary>
        /// Creates a new object if there is none in the pool.
        /// </summary>
        /// <returns>A new object.</returns>
        T Create();

        /// <summary>
        /// Executes cleanup on the returned object.
        /// </summary>
        /// <param name="obj">The object that will be cleaned up. </param>
        void Return(T obj);
    }
}
