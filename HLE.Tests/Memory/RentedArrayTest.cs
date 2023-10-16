using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Memory;

[TestClass]
public class RentedArrayTest
{
    [TestMethod]
    public void Indexer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array[0] = 0;
        array[1] = 1;
        array[2] = 2;
        array[3] = 3;
        Assert.IsTrue(array.AsSpan() is [0, 1, 2, 3, ..]);

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            array[-1] = 0;
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            array[array.Length] = 0;
        });
    }

    [TestMethod]
    public void IIndexAccessible_Indexer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array[0] = 0;
        array[1] = 1;
        array[2] = 2;
        array[3] = 3;

        IIndexAccessible<int> indexAccessible = array;
        Assert.AreEqual(0, indexAccessible[0]);
        Assert.AreEqual(1, indexAccessible[1]);
        Assert.AreEqual(2, indexAccessible[2]);
        Assert.AreEqual(3, indexAccessible[3]);

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            IIndexAccessible<int> indexAccessible = array;
            _ = indexAccessible[-1];
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            IIndexAccessible<int> indexAccessible = array;
            _ = indexAccessible[array.Length];
        });
    }

    [TestMethod]
    public void Indexer_Index_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array[new Index(0)] = 0;
        array[new Index(1)] = 1;
        array[^2] = 2;
        array[^1] = 3;

        Assert.IsTrue(array.AsSpan() is [0, 1, .., 2, 3]);

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            array[new Index(array.Length)] = 0;
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            array[^(array.Length + 1)] = 0;
        });
    }

    [TestMethod]
    public void Indexer_Range_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();

        Assert.IsTrue(array[..5] is [0, 1, 2, 3, 4, ..]);
        Assert.IsTrue(array[5..10] is [5, 6, 7, 8, 9, ..]);
        Assert.IsTrue(array[..^11] is [0, 1, 2, 3, 4, ..]);

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array[..(array.Length + 1)];
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array[(array.Length + 1)..];
        });
    }

    [TestMethod]
    public void Reference_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);

        array[0] = 5;
        ref int reference = ref array.Reference;
        Assert.AreEqual(5, reference);
        reference = 10;
        Assert.AreEqual(10, array[0]);

        ref int actualReference = ref MemoryMarshal.GetArrayDataReference(array.Array);
        Assert.IsTrue(Unsafe.AreSame(ref reference, ref actualReference));
    }

    [TestMethod]
    public void Length_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.AreEqual(array._array!.Length, array.Length);
        Assert.IsTrue(array.Length >= 16);
    }

    [TestMethod]
    public void ICountable_Count_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        ICountable countable = array;
        Assert.AreEqual(array.Length, countable.Count);
    }

    [TestMethod]
    public void ICollectionT_Count_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        ICollection<int> countable = array;
        Assert.AreEqual(array.Length, countable.Count);
    }

    [TestMethod]
    public void IReadOnlyCollectionCollectionT_Count_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        IReadOnlyCollection<int> countable = array;
        Assert.AreEqual(array.Length, countable.Count);
    }

    [TestMethod]
    public void ICollectionT_IsReadOnly_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        ICollection<int> countable = array;
        Assert.AreEqual(false, countable.IsReadOnly);
    }

    [TestMethod]
    public void Array_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.AreEqual(array._array, array.Array);
        Assert.IsTrue(ReferenceEquals(array._array, array.Array));
    }

    [TestMethod]
    public void Empty_Test()
    {
        using RentedArray<int> array = new();
        Assert.AreEqual(0, array.Length);
        Assert.AreEqual(Array.Empty<int>(), array._array);
        Assert.AreEqual(Array.Empty<int>(), array.Array);
        Assert.IsTrue(ReferenceEquals(Array.Empty<int>(), array._array));
        Assert.IsTrue(ReferenceEquals(Array.Empty<int>(), array.Array));
    }

    [TestMethod]
    public void Pool_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.AreEqual(ArrayPool<int>.Shared, array._pool);
        Assert.IsTrue(ReferenceEquals(ArrayPool<int>.Shared, array._pool));
    }

    [TestMethod]
    public void Dispose_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.Dispose();
        array.Dispose();
        array.Dispose();

        Assert.IsTrue(array is { _array: null });

        Assert.ThrowsException<ObjectDisposedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            array.Dispose();
            _ = array.Array;
        });

        Assert.ThrowsException<ObjectDisposedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            array.Dispose();
            array[5] = 123;
        });

        Assert.ThrowsException<ObjectDisposedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            array.Dispose();
            _ = array.AsSpan();
        });
    }

    [TestMethod]
    public void AsSpan_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsSpan() == array._array.AsSpan());
    }

    [TestMethod]
    public void AsSpan_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsSpan(5..) == array._array.AsSpan(5));

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array.AsSpan((array.Length + 1)..);
        });
    }

    [TestMethod]
    public void AsSpan_Integer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsSpan(5, 3) == array._array.AsSpan(5, 3));

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array.AsSpan(5, array.Length + 5);
        });
    }

    [TestMethod]
    public void AsSpan_Range_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsSpan(5..8) == array._array.AsSpan(5..8));

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array.AsSpan(5..(array.Length + 10));
        });
    }

    [TestMethod]
    public void AsMemory_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsMemory().Equals(array._array.AsMemory()));
    }

    [TestMethod]
    public void AsMemory_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsMemory(5..).Equals(array._array.AsMemory(5)));

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array.AsMemory((array.Length + 1)..);
        });
    }

    [TestMethod]
    public void AsMemory_Integer_Integer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsMemory(5, 3).Equals(array._array.AsMemory(5, 3)));

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array.AsMemory(5, array.Length + 5);
        });
    }

    [TestMethod]
    public void AsMemory_Range_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.AsMemory(5..8).Equals(array._array.AsMemory(5..8)));

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            _ = array.AsMemory(5..(array.Length + 10));
        });
    }

    [TestMethod]
    public void ISpanProvider_GetSpan()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        ISpanProvider<int> spanProvider = array;
        Assert.IsTrue(spanProvider.GetSpan() == array._array.AsSpan());
    }

    [TestMethod]
    public void IReadOnlySpanProvider_GetSpan()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        IReadOnlySpanProvider<int> spanProvider = array;
        Assert.IsTrue(spanProvider.GetReadOnlySpan() == array._array.AsSpan());
    }

    [TestMethod]
    public void CopyTo_List_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        List<int> destination = new();
        array.CopyTo(destination);
        Assert.IsTrue(CollectionsMarshal.AsSpan(destination).SequenceEqual(array.AsSpan()));

        destination = new();
        array.CopyTo(destination, 16);
        Assert.AreEqual(32, destination.Count);
        Assert.IsTrue(CollectionsMarshal.AsSpan(destination)[16..].SequenceEqual(array.AsSpan()));
    }

    [TestMethod]
    public void CopyTo_Array_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        int[] destination = new int[16];
        array.CopyTo(destination);
        Assert.IsTrue(destination.AsSpan().SequenceEqual(array.AsSpan()));

        destination = new int[32];
        array.CopyTo(destination, 16);
        Assert.IsTrue(destination.AsSpan(16).SequenceEqual(array.AsSpan()));
    }

    [TestMethod]
    public void CopyTo_Memory_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        Memory<int> destination = new int[16];
        array.CopyTo(destination);
        Assert.IsTrue(destination.Span.SequenceEqual(array.AsSpan()));
    }

    [TestMethod]
    public void CopyTo_Span_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        Span<int> destination = stackalloc int[16];
        array.CopyTo(destination);
        Assert.IsTrue(destination.SequenceEqual(array.AsSpan()));
    }

    [TestMethod]
    public void CopyTo_Reference_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        Span<int> destination = stackalloc int[16];
        array.CopyTo(ref MemoryMarshal.GetReference(destination));
        Assert.IsTrue(destination.SequenceEqual(array.AsSpan()));
    }

    [TestMethod]
    public unsafe void CopyTo_Pointer_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        int* destination = stackalloc int[16];
        array.CopyTo(destination);
        Assert.IsTrue(new Span<int>(destination, 16).SequenceEqual(array.AsSpan()));
    }

    [TestMethod]
    public void ICollectionT_Add_Test() =>
        Assert.ThrowsException<NotSupportedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            ICollection<int> collection = array;
            collection.Add(1);
        });

    [TestMethod]
    public void Clear_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        Assert.IsFalse(array.All(static i => i == 0));
        array.Clear();
        Assert.IsTrue(array.All(static i => i == 0));
    }

    [TestMethod]
    public void ICollectionT_Contains_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        ICollection<int> collection = array;
        Random.Shared.Fill(array.AsSpan());
        int firstValue = array[0];
        int lastValue = array[^1];

        Assert.IsTrue(collection.Contains(firstValue));
        Assert.IsTrue(collection.Contains(lastValue));

        array.AsSpan().Fill(5);
        Assert.IsFalse(collection.Contains(10));
    }

    [TestMethod]
    public void ICollectionT_Remove_Test() =>
        Assert.ThrowsException<NotSupportedException>(static () =>
        {
            using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
            ICollection<int> collection = array;
            collection.Remove(1);
        });

    [TestMethod]
    public void Equals_Object_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        object arrayAsObject = array;
        Assert.IsTrue(array.Equals(arrayAsObject));

        arrayAsObject = array.Array;
        Assert.IsTrue(array.Equals(arrayAsObject));

        object obj = new List<int>();
        Assert.IsFalse(array.Equals(obj));
    }

    [TestMethod]
    public void Equals_RentedArray_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.Equals(array));

        using RentedArray<int> array2 = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsFalse(array.Equals(array2));
    }

    [TestMethod]
    public void Equals_Array_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsTrue(array.Equals(array._array));

        int[] array2 = new int[16];
        Assert.IsFalse(array.Equals(array2));
    }

    [TestMethod]
    public void Enumerator_Test()
    {
        int counter = 0;
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        array.AsSpan().FillAscending();
        foreach (int i in array)
        {
            Assert.AreEqual(counter++, i);
        }
    }

    [TestMethod]
    public void EqualsOperator_RentedArray_Test()
    {
        using RentedArray<int> array = ArrayPool<int>.Shared.CreateRentedArray(16);
        // ReSharper disable once EqualExpressionComparison
#pragma warning disable CS1718 // Comparison made to same variable; did you mean to compare something else?
        Assert.IsTrue(array == array);
#pragma warning restore CS1718

        using RentedArray<int> array2 = ArrayPool<int>.Shared.CreateRentedArray(16);
        Assert.IsFalse(array == array2);
    }
}
