using System.Diagnostics.Contracts;
using HLE.Memory;

namespace HLE.Marshalling;

public static class RentedArrayMarshal<T>
{
    [Pure]
    public static T[] GetArray(RentedArray<T> rentedArray) => rentedArray.Array;

    [Pure]
    public static ArrayPool<T> GetPool(RentedArray<T> rentedArray) => rentedArray._pool;
}
