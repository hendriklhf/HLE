using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Resources;
using HLE.Test.TestUtilities;
using Xunit;

namespace HLE.Tests.Collections;

public sealed partial class CollectionHelpersTest
{
    public static TheoryData<int> ReplaceTestParameters { get; } = TheoryDataHelpers.CreateExclusiveRange(0, 256);

    public static TheoryData<int> TryNonEnumeratedCopyToParameters { get; } = new(0, 16, 32, 47);

    public static TheoryData<int> ToPooledListParameters { get; } = TheoryDataHelpers.CreateExclusiveRange(0, 256);

    private static bool ReplacePredicate(int i) => i == 1;

    [Theory]
    [MemberData(nameof(ReplaceTestParameters))]
    public void Replace_ReplaceEnumerable_Test(int length)
    {
        IEnumerable<int> items = Random.Shared.GetItems([0, 1], length);
        ReadOnlySpan<int> array = items.Replace(ReplacePredicate, 0).ToArray();
        Assert.False(array.ContainsAnyExcept(0));
    }

    [Theory]
    [MemberData(nameof(ReplaceTestParameters))]
    public void Replace_List_Test(int length)
    {
        List<int> list = new(length);
        CollectionsMarshal.SetCount(list, length);
        Random.Shared.Fill(list, [0, 1]);

        list.Replace(ReplacePredicate, 0);

        Assert.False(CollectionsMarshal.AsSpan(list).ContainsAnyExcept(0));
    }

    [Theory]
    [MemberData(nameof(ReplaceTestParameters))]
    public void Replace_Array_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array, [0, 1]);

        array.Replace(ReplacePredicate, 0);

        Assert.False(array.AsSpan().ContainsAnyExcept(0));
    }

    [Theory]
    [MemberData(nameof(ReplaceTestParameters))]
    public void Replace_Span_Test(int length)
    {
        Span<int> span = new int[length];
        Random.Shared.Fill(span, [0, 1]);

        span.Replace(ReplacePredicate, 0);

        Assert.False(span.ContainsAnyExcept(0));
    }

    [Theory]
    [MemberData(nameof(ReplaceTestParameters))]
    public unsafe void Replace_List_FunctionPointer_Test(int length)
    {
        List<int> list = new(length);
        CollectionsMarshal.SetCount(list, length);
        Random.Shared.Fill(list, [0, 1]);

        list.Replace(&ReplacePredicate, 0);

        Assert.False(CollectionsMarshal.AsSpan(list).ContainsAnyExcept(0));
    }

    [Theory]
    [MemberData(nameof(ReplaceTestParameters))]
    public unsafe void Replace_Array_FunctionPointer_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array, [0, 1]);

        array.Replace(&ReplacePredicate, 0);

        Assert.False(array.AsSpan().ContainsAnyExcept(0));
    }

    [Theory]
    [MemberData(nameof(ReplaceTestParameters))]
    public unsafe void Replace_Span_FunctionPointer_Test(int length)
    {
        Span<int> span = new int[length];
        Random.Shared.Fill(span, [0, 1]);

        span.Replace(&ReplacePredicate, 0);

        Assert.False(span.ContainsAnyExcept(0));
    }

    [Fact]
    public void RangeEnumerator_ZeroToHundred_Test()
    {
        using ValueList<int> items = new(stackalloc int[101]);
        Range r = ..100;
        foreach (int i in r)
        {
            items.Add(i);
        }

        Assert.Equal(101, items.Count);
        Assert.Equal(0, items[0]);
        Assert.Equal(100, items[^1]);
    }

    [Fact]
    public void RangeEnumerator_FiftyToHundred_Test()
    {
        using ValueList<int> items = new(stackalloc int[101]);
        Range r = 50..100;
        foreach (int i in r)
        {
            items.Add(i);
        }

        Assert.Equal(51, items.Count);
        Assert.Equal(50, items[0]);
        Assert.Equal(100, items[^1]);
    }

    [Fact]
    public void RangeEnumerator_ThrowInvalidOperationException_RangeDoesntHaveAnEnd() =>
        Assert.Throws<InvalidOperationException>(static () =>
        {
            foreach (int _ in 50..)
            {
                Nop();
            }
        });

    [Fact]
    public void RangeEnumerator_ThrowsInvalidOperationException_RangeStartsFromEnd() =>
        Assert.Throws<InvalidOperationException>(static () =>
        {
            foreach (int _ in ..^100)
            {
                Nop();
            }
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Nop()
    {
        // nop
    }

    [Fact]
    public void AddOrSet_Dictionary_Test()
    {
        Dictionary<int, string> dictionary = [];
        dictionary.AddOrSet(8, "hello");
        Assert.Equal("hello", dictionary[8]);

        dictionary.AddOrSet(8, "xd");
        Assert.Equal("xd", dictionary[8]);
    }

    [Fact]
    public void AddOrSet_ConcurrentDictionary_Test()
    {
        ConcurrentDictionary<int, string> dictionary = [];
        dictionary.AddOrSet(8, "hello");
        Assert.Equal("hello", dictionary[8]);

        dictionary.AddOrSet(8, "xd");
        Assert.Equal("xd", dictionary[8]);
    }

    [Fact]
    public void TryGetReadOnlySpan_YieldingEnumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        bool success = enumerable.TryGetReadOnlySpan(out _);
        Assert.False(success);
    }

    [Fact]
    public void TryGetReadOnlySpan_String_Test()
    {
        IEnumerable<char> enumerable = GetEnumerable<string, char>("hello");
        bool success = enumerable.TryGetReadOnlySpan(out ReadOnlySpan<char> span);
        Assert.True(success);
        Assert.True(Unsafe.AreSame(ref StringMarshal.GetReference("hello"), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetReadOnlySpan_Array_Test()
    {
        int[] array = Enumerable.Range(0, 8).ToArray();

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = enumerable.TryGetReadOnlySpan(out ReadOnlySpan<int> span);

        Assert.True(success);
        Assert.Equal(array.Length, span.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(array), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetReadOnlySpan_List_Test()
    {
        List<int> list = Enumerable.Range(0, 8).ToList();

        IEnumerable<int> enumerable = GetEnumerable<List<int>, int>(list);
        bool success = enumerable.TryGetReadOnlySpan(out ReadOnlySpan<int> span);

        Assert.True(success);
        Assert.Equal(list.Count, span.Length);
        Assert.True(Unsafe.AreSame(ref ListMarshal.GetReference(list), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetReadOnlySpan_ISpanProvider_Test()
    {
        using PooledList<int> list = Enumerable.Range(0, 8).ToPooledList();
        Assert.True(typeof(PooledList<int>).IsAssignableTo(typeof(ISpanProvider<int>)));

        IEnumerable<int> enumerable = GetEnumerable<PooledList<int>, int>(list);
        bool success = enumerable.TryGetReadOnlySpan(out ReadOnlySpan<int> span);

        Assert.True(success);
        Assert.Equal(list.Count, span.Length);
        Assert.True(Unsafe.AreSame(ref list.GetBufferReference(), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetReadOnlySpan_ImmutableArray_Test()
    {
        ImmutableArray<int> array = [.. Enumerable.Range(0, 8)];

        IEnumerable<int> enumerable = GetEnumerable<ImmutableArray<int>, int>(array);
        bool success = enumerable.TryGetReadOnlySpan(out ReadOnlySpan<int> span);

        Assert.True(success);
        Assert.Equal(array.Length, span.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(array.AsSpan()), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public unsafe void TryGetReadOnlySpan_IReadOnlySpanProvider_Test()
    {
        byte* buffer = stackalloc byte[8];
        SpanHelpers.FillAscending<byte>(ref Unsafe.AsRef<byte>(buffer), 8, 0);
        Resource resource = new(buffer, 8);

        Assert.True(typeof(Resource).IsAssignableTo(typeof(IReadOnlySpanProvider<byte>)));

        IEnumerable<byte> enumerable = GetEnumerable<Resource, byte>(resource);
        bool success = enumerable.TryGetReadOnlySpan(out ReadOnlySpan<byte> span);

        Assert.True(success);
        Assert.Equal(resource.Length, span.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(resource.AsSpan()), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetReadOnlySpan_FrozenSet_Test()
    {
        FrozenSet<int> frozenSet = Enumerable.Range(0, 8).ToFrozenSet();

        IEnumerable<int> enumerable = GetEnumerable<FrozenSet<int>, int>(frozenSet);
        bool success = enumerable.TryGetReadOnlySpan(out ReadOnlySpan<int> span);

        Assert.True(success);
        Assert.Equal(frozenSet.Count, span.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(frozenSet.Items.AsSpan()), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetSpan_YieldingEnumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        bool success = enumerable.TryGetSpan(out _);
        Assert.False(success);
    }

    [Fact]
    public void TryGetSpan_Array_Test()
    {
        int[] array = Enumerable.Range(0, 8).ToArray();

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = enumerable.TryGetSpan(out Span<int> span);

        Assert.True(success);
        Assert.Equal(array.Length, span.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(array), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetSpan_List_Test()
    {
        List<int> list = Enumerable.Range(0, 8).ToList();

        IEnumerable<int> enumerable = GetEnumerable<List<int>, int>(list);
        bool success = enumerable.TryGetSpan(out Span<int> span);

        Assert.True(success);
        Assert.Equal(list.Count, span.Length);
        Assert.True(Unsafe.AreSame(ref ListMarshal.GetReference(list), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetSpan_ISpanProvider_Test()
    {
        using PooledList<int> list = Enumerable.Range(0, 8).ToPooledList();
        Assert.True(typeof(PooledList<int>).IsAssignableTo(typeof(ISpanProvider<int>)));

        IEnumerable<int> enumerable = GetEnumerable<PooledList<int>, int>(list);
        bool success = enumerable.TryGetSpan(out Span<int> span);

        Assert.True(success);
        Assert.Equal(list.Count, span.Length);
        Assert.True(Unsafe.AreSame(ref list.GetBufferReference(), ref MemoryMarshal.GetReference(span)));
    }

    [Fact]
    public void TryGetReadOnlyMemory_YieldingEnumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        bool success = enumerable.TryGetReadOnlyMemory(out _);
        Assert.False(success);
    }

    [Fact]
    public void TryGetReadOnlyMemory_String_Test()
    {
        IEnumerable<char> enumerable = GetEnumerable<string, char>("hello");
        bool success = enumerable.TryGetReadOnlyMemory(out ReadOnlyMemory<char> memory);
        Assert.True(success);
        Assert.True(Unsafe.AreSame(ref StringMarshal.GetReference("hello"), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetReadOnlyMemory_Array_Test()
    {
        int[] array = Enumerable.Range(0, 8).ToArray();

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = enumerable.TryGetReadOnlyMemory(out ReadOnlyMemory<int> memory);

        Assert.True(success);
        Assert.Equal(array.Length, memory.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(array), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetReadOnlyMemory_List_Test()
    {
        List<int> list = Enumerable.Range(0, 8).ToList();

        IEnumerable<int> enumerable = GetEnumerable<List<int>, int>(list);
        bool success = enumerable.TryGetReadOnlyMemory(out ReadOnlyMemory<int> memory);

        Assert.True(success);
        Assert.Equal(list.Count, memory.Length);
        Assert.True(Unsafe.AreSame(ref ListMarshal.GetReference(list), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetReadOnlyMemory_IMemoryProvider_Test()
    {
        using PooledList<int> list = Enumerable.Range(0, 8).ToPooledList();
        Assert.True(typeof(PooledList<int>).IsAssignableTo(typeof(IMemoryProvider<int>)));

        IEnumerable<int> enumerable = GetEnumerable<PooledList<int>, int>(list);
        bool success = enumerable.TryGetReadOnlyMemory(out ReadOnlyMemory<int> memory);

        Assert.True(success);
        Assert.Equal(list.Count, memory.Length);
        Assert.True(Unsafe.AreSame(ref list.GetBufferReference(), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetReadOnlyMemory_ImmutableArray_Test()
    {
        ImmutableArray<int> array = [.. Enumerable.Range(0, 8)];

        IEnumerable<int> enumerable = GetEnumerable<ImmutableArray<int>, int>(array);
        bool success = enumerable.TryGetReadOnlyMemory(out ReadOnlyMemory<int> memory);

        Assert.True(success);
        Assert.Equal(array.Length, memory.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(array.AsMemory().Span), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public unsafe void TryGetReadOnlyMemory_IReadOnlyMemoryProvider_Test()
    {
        byte* buffer = stackalloc byte[8];
        SpanHelpers.FillAscending<byte>(ref Unsafe.AsRef<byte>(buffer), 8, 0);
        Resource resource = new(buffer, 8);

        Assert.True(typeof(Resource).IsAssignableTo(typeof(IReadOnlyMemoryProvider<byte>)));

        IEnumerable<byte> enumerable = GetEnumerable<Resource, byte>(resource);
        bool success = enumerable.TryGetReadOnlyMemory(out ReadOnlyMemory<byte> memory);

        Assert.True(success);
        Assert.Equal(resource.Length, memory.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(resource.AsMemory().Span), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetReadOnlyMemory_FrozenSet_Test()
    {
        FrozenSet<int> frozenSet = Enumerable.Range(0, 8).ToFrozenSet();

        IEnumerable<int> enumerable = GetEnumerable<FrozenSet<int>, int>(frozenSet);
        bool success = enumerable.TryGetReadOnlyMemory(out ReadOnlyMemory<int> memory);

        Assert.True(success);
        Assert.Equal(frozenSet.Count, memory.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetReference(frozenSet.Items.AsMemory().Span), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetMemory_YieldingEnumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        bool success = enumerable.TryGetMemory(out _);
        Assert.False(success);
    }

    [Fact]
    public void TryGetMemory_Array_Test()
    {
        int[] array = Enumerable.Range(0, 8).ToArray();

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = enumerable.TryGetMemory(out Memory<int> memory);

        Assert.True(success);
        Assert.Equal(array.Length, memory.Length);
        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(array), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetMemory_List_Test()
    {
        List<int> list = Enumerable.Range(0, 8).ToList();

        IEnumerable<int> enumerable = GetEnumerable<List<int>, int>(list);
        bool success = enumerable.TryGetMemory(out Memory<int> memory);

        Assert.True(success);
        Assert.Equal(list.Count, memory.Length);
        Assert.True(Unsafe.AreSame(ref ListMarshal.GetReference(list), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetMemory_IMemoryProvider_Test()
    {
        using PooledList<int> list = Enumerable.Range(0, 8).ToPooledList();
        Assert.True(typeof(PooledList<int>).IsAssignableTo(typeof(IMemoryProvider<int>)));

        IEnumerable<int> enumerable = GetEnumerable<PooledList<int>, int>(list);
        bool success = enumerable.TryGetMemory(out Memory<int> memory);

        Assert.True(success);
        Assert.Equal(list.Count, memory.Length);
        Assert.True(Unsafe.AreSame(ref list.GetBufferReference(), ref MemoryMarshal.GetReference(memory.Span)));
    }

    [Fact]
    public void TryGetNonEnumeratedCount_YieldingEnumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        bool success = CollectionHelpers.TryGetNonEnumeratedCount(enumerable, out _);
        Assert.False(success);
    }

    [Fact]
    public void TryGetNonEnumeratedCount_Array_Test()
    {
        int[] array = new int[8];

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = CollectionHelpers.TryGetNonEnumeratedCount(enumerable, out int count);

        Assert.True(success);
        Assert.Equal(array.Length, count);
    }

    [Fact]
    public void TryGetNonEnumeratedCount_ICountable_Test()
    {
        SomeCountable someCountable = new();

        IEnumerable<int> enumerable = GetEnumerable<SomeCountable, int>(someCountable);
        bool success = CollectionHelpers.TryGetNonEnumeratedCount(enumerable, out int count);

        Assert.True(success);
        Assert.Equal(someCountable.Count, count);
    }

    [Fact]
    public void TryGetNonEnumeratedCount_IReadOnlyCollection_Test()
    {
        SomeReadOnlyCollection someReadOnlyCollection = new();

        IEnumerable<int> enumerable = GetEnumerable<SomeReadOnlyCollection, int>(someReadOnlyCollection);
        bool success = CollectionHelpers.TryGetNonEnumeratedCount(enumerable, out int count);

        Assert.True(success);
        Assert.Equal(someReadOnlyCollection.Count, count);
    }

    [Fact]
    public void TryNonEnumeratedCopyTo_YieldingEnumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        int[] destination = new int[8];
        bool success = enumerable.TryNonEnumeratedCopyTo(destination, 0, out _);
        Assert.False(success);
    }

    [Theory]
    [MemberData(nameof(TryNonEnumeratedCopyToParameters))]
    public void TryNonEnumeratedCopyTo_Array_Test(int writeOffset)
    {
        const int SourceLength = 16;

        int[] array = new int[SourceLength];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        int[] destination = new int[SourceLength + writeOffset];
        bool success = enumerable.TryNonEnumeratedCopyTo(destination, writeOffset, out int elementsCopied);

        Assert.True(success);
        Assert.Equal(elementsCopied, array.Length);
        Assert.True(array.AsSpan().SequenceEqual(destination.AsSpan(writeOffset..)));
    }

    [Theory]
    [MemberData(nameof(TryNonEnumeratedCopyToParameters))]
    public void TryNonEnumeratedCopyTo_ICollection_Test(int writeOffset)
    {
        const int SourceLength = 16;

        SomeCollection someCollection = new(SourceLength);

        IEnumerable<int> enumerable = GetEnumerable<SomeCollection, int>(someCollection);
        int[] destination = new int[SourceLength + writeOffset];
        bool success = enumerable.TryNonEnumeratedCopyTo(destination, writeOffset, out int elementsCopied);

        Assert.True(success);
        Assert.Equal(elementsCopied, someCollection.Count);
        Assert.True(someCollection.AsSpan().SequenceEqual(destination.AsSpan(writeOffset..)));
    }

    [Theory]
    [MemberData(nameof(TryNonEnumeratedCopyToParameters))]
    public void TryNonEnumeratedCopyTo_ICopyable_Test(int writeOffset)
    {
        const int SourceLength = 16;

        SomeCopyable someCopyable = new(SourceLength);

        IEnumerable<int> enumerable = GetEnumerable<SomeCopyable, int>(someCopyable);
        int[] destination = new int[SourceLength + writeOffset];
        bool success = enumerable.TryNonEnumeratedCopyTo(destination, writeOffset, out int elementsCopied);

        Assert.True(success);
        Assert.Equal(elementsCopied, someCopyable.Count);
        Assert.True(someCopyable.AsSpan().SequenceEqual(destination.AsSpan(writeOffset..)));
    }

    [Fact]
    public void TryNonEnumeratedElementAt_YieldingEnumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        bool success = enumerable.TryGetNonEnumeratedElementAt(0, out _);
        Assert.False(success);
    }

    [Fact]
    public void TryNonEnumeratedElementAt_Array_Test()
    {
        int[] array = new int[4];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = enumerable.TryGetNonEnumeratedElementAt(2, out int element);

        Assert.True(success);
        Assert.Equal(element, array[2]);
    }

    [Fact]
    public void TryNonEnumeratedElementAt_IList_Test()
    {
        SomeList list = new(4);

        IEnumerable<int> enumerable = GetEnumerable<SomeList, int>(list);
        bool success = enumerable.TryGetNonEnumeratedElementAt(2, out int element);

        Assert.True(success);
        Assert.Equal(element, list[2]);
    }

    [Fact]
    public void TryNonEnumeratedElementAt_IReadOnlyList_Test()
    {
        SomeReadOnlyList list = new(4);

        IEnumerable<int> enumerable = GetEnumerable<SomeReadOnlyList, int>(list);
        bool success = enumerable.TryGetNonEnumeratedElementAt(2, out int element);

        Assert.True(success);
        Assert.Equal(element, list[2]);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<TElement> GetEnumerable<TSource, TElement>(TSource source) where TSource : IEnumerable<TElement>
        => source;

    [Fact]
    public void MoveItem_List_SameIndex_Test()
    {
        List<int> list = [0, 1, 2, 3, 4, 5];
        list.MoveItem(3, 3);
        Assert.True(list is [0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void MoveItem_List_IndexNextToEachOther_Test()
    {
        List<int> list = [0, 1, 2, 3, 4, 5];
        list.MoveItem(3, 4);
        Assert.True(list is [0, 1, 2, 4, 3, 5]);
    }

    [Fact]
    public void MoveItem_List_BackToFront_Test()
    {
        List<int> list = [0, 1, 2, 3, 4, 5];
        list.MoveItem(4, 1);
        Assert.True(list is [0, 4, 1, 2, 3, 5]);
    }

    [Fact]
    public void MoveItem_List_FrontToBack_Test()
    {
        List<int> list = [0, 1, 2, 3, 4, 5];
        list.MoveItem(1, 4);
        Assert.True(list is [0, 2, 3, 4, 1, 5]);
    }

    [Fact]
    public void MoveItem_Array_SameIndex_Test()
    {
        int[] array = [0, 1, 2, 3, 4, 5];
        array.MoveItem(3, 3);
        Assert.True(array is [0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void MoveItem_Array_IndexNextToEachOther_Test()
    {
        int[] array = [0, 1, 2, 3, 4, 5];
        array.MoveItem(3, 4);
        Assert.True(array is [0, 1, 2, 4, 3, 5]);
    }

    [Fact]
    public void MoveItem_Array_BackToFront_Test()
    {
        int[] array = [0, 1, 2, 3, 4, 5];
        array.MoveItem(4, 1);
        Assert.True(array is [0, 4, 1, 2, 3, 5]);
    }

    [Fact]
    public void MoveItem_Array_FrontToBack_Test()
    {
        int[] array = [0, 1, 2, 3, 4, 5];
        array.MoveItem(1, 4);
        Assert.True(array is [0, 2, 3, 4, 1, 5]);
    }

    [Fact]
    public void MoveItem_Span_SameIndex_Test()
    {
        Span<int> span = [0, 1, 2, 3, 4, 5];
        span.MoveItem(3, 3);
        Assert.True(span is [0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void MoveItem_Span_IndexNextToEachOther_Test()
    {
        Span<int> span = [0, 1, 2, 3, 4, 5];
        span.MoveItem(3, 4);
        Assert.True(span is [0, 1, 2, 4, 3, 5]);
    }

    [Fact]
    public void MoveItem_Span_BackToFront_Test()
    {
        Span<int> span = [0, 1, 2, 3, 4, 5];
        span.MoveItem(4, 1);
        Assert.True(span is [0, 4, 1, 2, 3, 5]);
    }

    [Fact]
    public void MoveItem_Span_FrontToBack_Test()
    {
        Span<int> span = [0, 1, 2, 3, 4, 5];
        span.MoveItem(1, 4);
        Assert.True(span is [0, 2, 3, 4, 1, 5]);
    }

    [Fact]
    public void MoveItem_List_Throws_ArgumentOutOfRangeException() =>
        Assert.Throws<IndexOutOfRangeException>(static () =>
        {
            List<int> list = [];
            list.MoveItem(1, 4);
        });

    [Fact]
    public void MoveItem_Array_Throws_ArgumentOutOfRangeException() =>
        Assert.Throws<IndexOutOfRangeException>(static () =>
        {
            int[] array = [];
            array.MoveItem(1, 4);
        });

    [Fact]
    public void MoveItem_Span_Throws_ArgumentOutOfRangeException() =>
        Assert.Throws<IndexOutOfRangeException>(static () =>
        {
            Span<int> span = [];
            span.MoveItem(1, 4);
        });

    [Fact]
    public void TryEnumerateInto_List_Test()
    {
        List<int> list = [0, 1, 2, 3, 4, 5, 6, 7];
        Span<int> destination = stackalloc int[8];

        IEnumerable<int> enumerable = GetEnumerable<List<int>, int>(list);
        bool success = enumerable.TryEnumerateInto(destination, out int writtenElements);

        Assert.True(success);
        Assert.Equal(list.Count, writtenElements);
        Assert.True(CollectionsMarshal.AsSpan(list).SequenceEqual(destination));
    }

    [Fact]
    public void TryEnumerateInto_Array_Test()
    {
        int[] array = [0, 1, 2, 3, 4, 5, 6, 7];
        Span<int> destination = stackalloc int[8];

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = enumerable.TryEnumerateInto(destination, out int writtenElements);

        Assert.True(success);
        Assert.Equal(array.Length, writtenElements);
        Assert.True(array.AsSpan().SequenceEqual(destination));
    }

    [Fact]
    public void TryEnumerateInto_Array_DestinationTooShort_Test()
    {
        int[] array = [0, 1, 2, 3, 4, 5, 6, 7];
        Span<int> destination = stackalloc int[7];

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        bool success = enumerable.TryEnumerateInto(destination, out int writtenElements);

        Assert.False(success);
        Assert.Equal(0, writtenElements);
    }

    [Fact]
    public void TryEnumerateInto_Copyable_Test()
    {
        SomeCopyable someCopyable = new(8);
        Span<int> destination = stackalloc int[8];

        IEnumerable<int> enumerable = GetEnumerable<SomeCopyable, int>(someCopyable);
        bool success = enumerable.TryEnumerateInto(destination, out int writtenElements);

        Assert.True(success);
        Assert.Equal(someCopyable.Count, writtenElements);
        Assert.True(someCopyable.AsSpan().SequenceEqual(destination));
    }

    [Fact]
    public void TryEnumerateInto_Indexable_Test()
    {
        SomeIndexable someIndexable = new(8);
        Span<int> destination = stackalloc int[8];

        IEnumerable<int> enumerable = GetEnumerable<SomeIndexable, int>(someIndexable);
        bool success = enumerable.TryEnumerateInto(destination, out int writtenElements);

        Assert.True(success);
        Assert.Equal(someIndexable.Count, writtenElements);
        Assert.True(someIndexable.AsSpan().SequenceEqual(destination));
    }

    [Fact]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of \'IEnumerable\' collection")]
    public void TryEnumerateInto_Enumerable_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        int[] destination = new int[8];

        bool success = enumerable.TryEnumerateInto(destination, out int writtenElements);

        Assert.True(success);
        Assert.Equal(enumerable.Count(), writtenElements);
        Assert.True(enumerable.SequenceEqual(destination));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<int> GetYieldingEnumerable(int[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            yield return items[i];
        }
    }

    [Fact]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of \'IEnumerable\' collection")]
    public void TryEnumerateInto_Enumerable_DestinationTooShort_Test()
    {
        IEnumerable<int> enumerable = GetYieldingEnumerable([0, 1, 2, 3, 4, 5, 6, 7]);
        int[] destination = new int[7];

        bool success = enumerable.TryEnumerateInto(destination, out int writtenElements);

        Assert.False(success);
        Assert.Equal(destination.Length, writtenElements);
        Assert.True(enumerable.Take(7).SequenceEqual(destination));
    }

    [Theory]
    [MemberData(nameof(ToPooledListParameters))]
    public void ToPooledList_Array_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        using PooledList<int> list = enumerable.ToPooledList();

        Assert.Equal(length, list.Count);
        Assert.True(list.AsSpan().SequenceEqual(array));
    }

    [Theory]
    [MemberData(nameof(ToPooledListParameters))]
    public void ToPooledList_YieldingEnumerable_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetYieldingEnumerable(array);
        using PooledList<int> list = enumerable.ToPooledList();

        Assert.Equal(length, list.Count);
        Assert.True(list.AsSpan().SequenceEqual(array));
    }

    [Theory]
    [MemberData(nameof(ToPooledListParameters))]
    public void ToPooledBufferWriter_Array_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        using PooledBufferWriter<int> bufferWriter = enumerable.ToPooledBufferWriter();

        Assert.Equal(length, bufferWriter.Count);
        Assert.True(bufferWriter.WrittenSpan.SequenceEqual(array));
    }

    [Theory]
    [MemberData(nameof(ToPooledListParameters))]
    public void ToPooledBufferWriter_YieldingEnumerable_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetYieldingEnumerable(array);
        using PooledBufferWriter<int> bufferWriter = enumerable.ToPooledBufferWriter();

        Assert.Equal(length, bufferWriter.Count);
        Assert.True(bufferWriter.WrittenSpan.SequenceEqual(array));
    }

    [Theory]
    [MemberData(nameof(ToPooledListParameters))]
    public void ToRentedArray_Array_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetEnumerable<int[], int>(array);
        using RentedArray<int> rentedArray = enumerable.ToRentedArray();

        Assert.Equal(length, rentedArray.Length);
        Assert.True(rentedArray.AsSpan().SequenceEqual(array));
    }

    [Theory]
    [MemberData(nameof(ToPooledListParameters))]
    public void ToRentedArray_YieldingEnumerable_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        IEnumerable<int> enumerable = GetYieldingEnumerable(array);
        using RentedArray<int> rentedArray = enumerable.ToRentedArray();

        Assert.Equal(length, rentedArray.Length);
        Assert.True(rentedArray.AsSpan().SequenceEqual(array));
    }
}
