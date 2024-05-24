using System;
using System.Collections.Generic;

namespace HLE.Collections;

public interface ICollectionBuilder<out TCollection, TItem>
{
    static abstract TCollection Create(IEnumerable<TItem> items);

    static abstract TCollection Create(List<TItem> items);

    static abstract TCollection Create(TItem[] items);

    static abstract TCollection Create(Span<TItem> items);

    static abstract TCollection Create(params ReadOnlySpan<TItem> items);

    static abstract TCollection Create(TItem item);

    static abstract TCollection Create(TItem item0, TItem item1);

    static abstract TCollection Create(TItem item0, TItem item1, TItem item2);

    static abstract TCollection Create(TItem item0, TItem item1, TItem item2, TItem item3);

    static abstract TCollection Create(TItem item0, TItem item1, TItem item2, TItem item3, TItem item4);

    static abstract TCollection Create(TItem item0, TItem item1, TItem item2, TItem item3, TItem item4, TItem item5);

    static abstract TCollection Create(TItem item0, TItem item1, TItem item2, TItem item3, TItem item4, TItem item5, TItem item6);

    static abstract TCollection Create(TItem item0, TItem item1, TItem item2, TItem item3, TItem item4, TItem item5, TItem item6, TItem item7);
}
