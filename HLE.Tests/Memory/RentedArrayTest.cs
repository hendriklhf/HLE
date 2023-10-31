using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed class RentedArrayTest
{
    [Fact]
    public void Indexer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array[0] = 0;
        array[1] = 1;
        array[2] = 2;
        array[3] = 3;
        Assert.True(array.AsSpan() is [0, 1, 2, 3, ..]);

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            array[-1] = 0;
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            array[array.Length] = 0;
        });
    }

    [Fact]
    public void IIndexAccessible_Indexer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array[0] = 0;
        array[1] = 1;
        array[2] = 2;
        array[3] = 3;

        IIndexAccessible<int> indexAccessible = array;
        Assert.Equal(0, indexAccessible[0]);
        Assert.Equal(1, indexAccessible[1]);
        Assert.Equal(2, indexAccessible[2]);
        Assert.Equal(3, indexAccessible[3]);

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            IIndexAccessible<int> indexAccessible = array;
            _ = indexAccessible[-1];
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            IIndexAccessible<int> indexAccessible = array;
            _ = indexAccessible[array.Length];
        });
    }

    [Fact]
    public void Indexer_Index_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array[new Index(0)] = 0;
        array[new Index(1)] = 1;
        array[^2] = 2;
        array[^1] = 3;

        Assert.True(array.AsSpan() is [0, 1, .., 2, 3]);

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            array[new Index(array.Length)] = 0;
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            array[^(array.Length + 1)] = 0;
        });
    }

    [Fact]
    public void Indexer_Range_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();

        Assert.True(array[..5] is [0, 1, 2, 3, 4, ..]);
        Assert.True(array[5..10] is [5, 6, 7, 8, 9, ..]);
        Assert.True(array[..^11] is [0, 1, 2, 3, 4, ..]);

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array[..(array.Length + 1)];
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array[(array.Length + 1)..];
        });
    }

    [Fact]
    public void Reference_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);

        array[0] = 5;
        ref int reference = ref array.Reference;
        Assert.Equal(5, reference);
        reference = 10;
        Assert.Equal(10, array[0]);

        ref int actualReference = ref MemoryMarshal.GetArrayDataReference(array.Array);
        Assert.True(Unsafe.AreSame(ref reference, ref actualReference));
    }

    [Fact]
    public void Length_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.Equal(array._array!.Length, array.Length);
        Assert.True(array.Length >= 16);
    }

    [Fact]
    public void ICountable_Count_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        ICountable countable = array;
        Assert.Equal(array.Length, countable.Count);
    }

    [Fact]
    public void ICollectionT_Count_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        ICollection<int> countable = array;
        Assert.Equal(array.Length, countable.Count);
    }

    [Fact]
    public void IReadOnlyCollectionCollectionT_Count_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        IReadOnlyCollection<int> countable = array;
        Assert.Equal(array.Length, countable.Count);
    }

    [Fact]
    public void ICollectionT_IsReadOnly_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        ICollection<int> collection = array;
        Assert.False(collection.IsReadOnly);
    }

    [Fact]
    public void Array_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.Equal(array._array, array.Array);
        Assert.Same(array._array, array.Array);
    }

    [Fact]
    public void Empty_Test()
    {
        using RentedArray<int> array = new();
        Assert.Equal(0, array.Length);
        Assert.Equal([], array._array);
        Assert.Equal([], array.Array);
        Assert.Same(Array.Empty<int>(), array._array);
        Assert.Same(Array.Empty<int>(), array.Array);
    }

    [Fact]
    public void Pool_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.Same(ArrayPool<int>.Shared, array._pool);
    }

    [Fact]
    public void Dispose_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.Dispose();
        array.Dispose();
        array.Dispose();

        Assert.Null(array._array);

        Assert.Throws<ObjectDisposedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            array.Dispose();
            _ = array.Array;
        });

        Assert.Throws<ObjectDisposedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            array.Dispose();
            array[5] = 123;
        });

        Assert.Throws<ObjectDisposedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            array.Dispose();
            _ = array.AsSpan();
        });
    }

    [Fact]
    public void AsSpan_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsSpan() == array._array.AsSpan());
    }

    [Fact]
    public void AsSpan_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsSpan(5..) == array._array.AsSpan(5));

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array.AsSpan((array.Length + 1)..);
        });
    }

    [Fact]
    public void AsSpan_Integer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsSpan(5, 3) == array._array.AsSpan(5, 3));

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array.AsSpan(5, array.Length + 5);
        });
    }

    [Fact]
    public void AsSpan_Range_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsSpan(5..8) == array._array.AsSpan(5..8));

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array.AsSpan(5..(array.Length + 10));
        });
    }

    [Fact]
    public void AsMemory_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsMemory().Equals(array._array.AsMemory()));
    }

    [Fact]
    public void AsMemory_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsMemory(5..).Equals(array._array.AsMemory(5)));

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array.AsMemory((array.Length + 1)..);
        });
    }

    [Fact]
    public void AsMemory_Integer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsMemory(5, 3).Equals(array._array.AsMemory(5, 3)));

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array.AsMemory(5, array.Length + 5);
        });
    }

    [Fact]
    public void AsMemory_Range_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.AsMemory(5..8).Equals(array._array.AsMemory(5..8)));

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            _ = array.AsMemory(5..(array.Length + 10));
        });
    }

    [Fact]
    public void ISpanProvider_GetSpan()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        ISpanProvider<int> spanProvider = array;
        Assert.True(spanProvider.GetSpan() == array._array.AsSpan());
    }

    [Fact]
    public void IReadOnlySpanProvider_GetSpan()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        IReadOnlySpanProvider<int> spanProvider = array;
        Assert.True(spanProvider.GetReadOnlySpan() == array._array.AsSpan());
    }

    [Fact]
    public void CopyTo_List_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        List<int> destination = [];
        array.CopyTo(destination);
        Assert.True(CollectionsMarshal.AsSpan(destination).SequenceEqual(array.AsSpan()));

        destination = [];
        array.CopyTo(destination, 16);
        Assert.Equal(32, destination.Count);
        Assert.True(CollectionsMarshal.AsSpan(destination)[16..].SequenceEqual(array.AsSpan()));
    }

    [Fact]
    public void CopyTo_Array_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        int[] destination = new int[16];
        array.CopyTo(destination);
        Assert.True(destination.AsSpan().SequenceEqual(array.AsSpan()));

        destination = new int[32];
        array.CopyTo(destination, 16);
        Assert.True(destination.AsSpan(16).SequenceEqual(array.AsSpan()));
    }

    [Fact]
    public void CopyTo_Memory_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        Memory<int> destination = new int[16];
        array.CopyTo(destination);
        Assert.True(destination.Span.SequenceEqual(array.AsSpan()));
    }

    [Fact]
    public void CopyTo_Span_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        Span<int> destination = stackalloc int[16];
        array.CopyTo(destination);
        Assert.True(destination.SequenceEqual(array.AsSpan()));
    }

    [Fact]
    public void CopyTo_Reference_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        Span<int> destination = stackalloc int[16];
        array.CopyTo(ref MemoryMarshal.GetReference(destination));
        Assert.True(destination.SequenceEqual(array.AsSpan()));
    }

    [Fact]
    public unsafe void CopyTo_Pointer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        int* destination = stackalloc int[16];
        array.CopyTo(destination);
        Assert.True(new Span<int>(destination, 16).SequenceEqual(array.AsSpan()));
    }

    [Fact]
    public void ICollectionT_Add_Test() =>
        Assert.Throws<NotSupportedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            ICollection<int> collection = array;
            collection.Add(1);
        });

    [Fact]
    public void Clear_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        Assert.False(array.All(static i => i == 0));
        array.Clear();
        Assert.True(array.All(static i => i == 0));
    }

    [Fact]
    public void ICollectionT_Contains_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        ICollection<int> collection = array;
        Random.Shared.Fill(array.AsSpan());
        int firstValue = array[0];
        int lastValue = array[^1];

        Assert.True(collection.Contains(firstValue));
        Assert.True(collection.Contains(lastValue));

        array.AsSpan().Fill(5);
        Assert.False(collection.Contains(10));
    }

    [Fact]
    public void ICollectionT_Remove_Test() =>
        Assert.Throws<NotSupportedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
            ICollection<int> collection = array;
            collection.Remove(1);
        });

    [Fact]
    public void Equals_Object_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        object arrayAsObject = array;
        Assert.True(array.Equals(arrayAsObject));

        arrayAsObject = array.Array;
        Assert.True(array.Equals(arrayAsObject));

        object obj = new List<int>();
        Assert.False(array.Equals(obj));
    }

    [Fact]
    public void Equals_RentedArray_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.Equals(array));

        using RentedArray<int> array2 = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.False(array.Equals(array2));
    }

    [Fact]
    public void Equals_Array_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.True(array.Equals(array._array));

        int[] array2 = new int[16];
        Assert.False(array.Equals(array2));
    }

    [Fact]
    public void Enumerator_Test()
    {
        int counter = 0;
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        array.AsSpan().FillAscending();
        foreach (int i in array)
        {
            Assert.Equal(counter++, i);
        }

        Assert.Equal(array.Length, counter);
    }

    [Fact]
    public void EqualsOperator_RentedArray_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.RentAsRentedArray(16);
        // ReSharper disable once EqualExpressionComparison
#pragma warning disable CS1718 // Comparison made to same variable; did you mean to compare something else?
        Assert.True(array == array);
#pragma warning restore CS1718

        using RentedArray<int> array2 = ArrayPool<int>.Shared.RentAsRentedArray(16);
        Assert.False(array == array2);
    }
}
