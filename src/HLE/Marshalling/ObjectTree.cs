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
    private static readonly MethodInfo s_sizeOfMethod = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly ConcurrentDictionary<Type, nuint> s_valueTypeSizeCache = new();

    [Pure]
    public static nuint GetSize(object? obj)
    {
        if (obj is null)
        {
            return 0;
        }

        Type objectType = obj.GetType();
        if (!ObjectMarshal.IsReferenceOrContainsReference(objectType))
        {
            return GetValueTypeSize(obj);
        }

        nuint size;
        if (objectType.IsValueType)
        {
            size = GetValueTypeSize(objectType);
            if (IsInlineArray(objectType, out int length))
            {
                return GetInlineArrayElementsSize(obj, length, objectType);
            }
        }
        else
        {
            size = ObjectMarshal.GetObjectSize(obj);
            if (objectType == typeof(string))
            {
                return size;
            }

            if (objectType.IsArray)
            {
                return size + GetArrayElementsSize(Unsafe.As<Array>(obj), objectType);
            }
        }

        ReadOnlySpan<FieldInfo> instanceFields = GetFields(objectType);
        foreach (FieldInfo field in instanceFields)
        {
            if (!ObjectMarshal.IsReferenceOrContainsReference(field.FieldType))
            {
                continue;
            }

            object? fieldValue = field.GetValue(obj);
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

    private static nuint GetValueTypeSize(object obj)
    {
        Type valueType = obj.GetType();
        if (s_valueTypeSizeCache.TryGetValue(valueType, out nuint size))
        {
            return size;
        }

        MethodInfo sizeOfMethod = s_sizeOfMethod.MakeGenericMethod(valueType);
        size = (uint)(int)sizeOfMethod.Invoke(null, null)!;
        s_valueTypeSizeCache.TryAdd(valueType, size);
        return size;
    }

    private static nuint GetArrayElementsSize(Array array, Type arrayType)
    {
        Type elementType = arrayType.GetElementType()!;
        if (!ObjectMarshal.IsReferenceOrContainsReference(elementType))
        {
            return 0;
        }

        nuint size = 0;
        foreach (object? obj in array)
        {
            size += GetSize(obj);
        }

        return size;
    }

    private static bool IsInlineArray(Type type, out int arrayLength)
    {
        InlineArrayAttribute? attribute = type.GetCustomAttribute<InlineArrayAttribute>();
        if (attribute is null)
        {
            arrayLength = 0;
            return false;
        }

        arrayLength = attribute.Length;
        return true;
    }

    private static nuint GetInlineArrayElementsSize(object array, int length, Type type)
    {
        _ = array;
        _ = length;
        _ = type;
        FieldInfo inlineArrayField = GetFields(type)[0];
        Debug.Assert(ObjectMarshal.IsReferenceOrContainsReference(inlineArrayField.FieldType));
        return 0; // TODO: implement correct inline array calculation
    }
}
