using System.Diagnostics.Contracts;
using HLE.Memory;

namespace HLE.Marshalling;

public static class RentedArrayMarshal
{
    [Pure]
    public static T[] GetArray<T>(RentedArray<T> rentedArray) => rentedArray.Array;
}
