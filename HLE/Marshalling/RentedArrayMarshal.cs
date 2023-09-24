using System.Diagnostics.Contracts;
using HLE.Memory;

namespace HLE.Marshalling;

public static class RentedArrayMarshal<T>
{
    [Pure]
    public static T[] GetArray(RentedArray<T> rentedArray)
    {
        return rentedArray.Array;
    }
}
