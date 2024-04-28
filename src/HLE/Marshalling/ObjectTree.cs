using System;
using System.Collections.Concurrent;
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
    public static nuint GetAllocationSize(object obj)
    {
        nuint size;
        Type objectType = obj.GetType();
        if (objectType.IsValueType)
        {
            size = GetValueTypeSize(obj);
            if (objectType.IsPrimitive)
            {
                return size;
            }
        }
        else
        {
            size = ObjectMarshal.GetObjectSize(obj);
            if (objectType.IsArray || objectType == typeof(string))
            {
                return size;
            }
        }

        ReadOnlySpan<FieldInfo> instanceFields = GetFields(objectType);
        foreach (FieldInfo field in instanceFields)
        {
            object? fieldValue = field.GetValue(obj);
            size += fieldValue is not null ? GetAllocationSize(fieldValue) : 0;
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
}
