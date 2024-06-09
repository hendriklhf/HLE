using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HLE.Marshalling;

internal static class ObjectTree
{
    private static readonly ConcurrentDictionary<Type, FieldInfo[]> s_fieldInfoCache = new();

    [Pure]
    public static unsafe nuint GetSize<T>(T obj)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return (uint)sizeof(T);
        }

        if (obj is null)
        {
            return 0;
        }

        nuint size;
        if (typeof(T).IsValueType)
        {
            size = (uint)sizeof(T);
            if (IsInlineArray<T>(out int length))
            {
                return size + GetInlineArrayElementsSize(obj, length);
            }
        }
        else
        {
            size = ObjectMarshal.GetObjectSize(obj);
            if (typeof(T) == typeof(string))
            {
                return size;
            }

            if (typeof(T).IsArray)
            {
                Type elementType = typeof(T).GetElementType()!;
                if (!ObjectMarshal.IsReferenceOrContainsReference(elementType))
                {
                    return size + GetArrayElementsSize(Unsafe.As<Array>(obj), elementType);
                }

                return size;
            }
        }

        return size + GetFieldsSize(obj, typeof(T));
    }

    private static nuint GetFieldsSize<T>(T obj, Type type)
    {
        nuint size = 0;
        ReadOnlySpan<FieldInfo> instanceFields = GetFields(type);
        foreach (FieldInfo field in instanceFields)
        {
            if (!ObjectMarshal.IsReferenceOrContainsReference(field.FieldType))
            {
                continue;
            }

            object? fieldValue = field.GetValue(obj); // TODO: boxes obj and the field's value
            size += fieldValue is not null ? GetSize(fieldValue) : 0;
        }

        return size;
    }

    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
    private static ReadOnlySpan<FieldInfo> GetFields(Type type)
    {
        if (s_fieldInfoCache.TryGetValue(type, out FieldInfo[]? fieldInfos))
        {
            return fieldInfos;
        }

        fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        s_fieldInfoCache.TryAdd(type, fieldInfos);
        return fieldInfos;
    }

    private static nuint GetArrayElementsSize(Array array, Type elementType)
    {
        Debug.Assert(ObjectMarshal.IsReferenceOrContainsReference(array.GetType().GetElementType()!));

        nuint size = 0;
        if (!elementType.IsValueType)
        {
            foreach (object? obj in array)
            {
                size += GetSize(obj);
            }
        }
        else
        {
            foreach (object? obj in array) // boxes the array elements
            {
                size += GetFieldsSize(obj, elementType);
            }
        }

        return size;
    }

    private static bool IsInlineArray<T>(out int arrayLength)
    {
        InlineArrayAttribute? attribute = typeof(T).GetCustomAttribute<InlineArrayAttribute>();
        if (attribute is null)
        {
            arrayLength = 0;
            return false;
        }

        arrayLength = attribute.Length;
        return true;
    }

    private static nuint GetInlineArrayElementsSize<T>(T array, int length)
    {
        _ = array;
        _ = length;
        FieldInfo inlineArrayField = GetFields(typeof(T))[0];
        _ = inlineArrayField;
        return 0; // TODO: implement correct inline array calculation
    }
}
