
.assembly extern System.Runtime
{
    .ver 9:0:0:0
    .publickeytoken = ( B0 3F 5F 7F 11 D5 0A 3A )
}

.assembly HLE.IL
{
    .ver 0:0:0:0
}

.module HLE.IL

.class public abstract auto ansi sealed beforefieldinit HLE.IL.UnsafeIL extends [System.Runtime]System.Object
{
    .method public hidebysig static !!T& GetField<T>(object obj, native unsigned int byteOffset) cil managed aggressiveinlining
    {
        ldarg.0
        ldarg.1
        add
        ret
    }

    .method public hidebysig static char& GetStringReference(string str) cil managed aggressiveinlining
    {
        ldarg.0
        sizeof [System.Runtime]System.UIntPtr
        add
        ldc.i4.4
        add
        ret
    }

    .method public hidebysig static !!T& GetArrayReference<T>(!!0[] 'array') cil managed aggressiveinlining
    {
        ldarg.0
        sizeof [System.Runtime]System.UIntPtr
        add
        sizeof [System.Runtime]System.UIntPtr
        add
        ret
    }

    .method public hidebysig static string AsString(char& chars) cil managed aggressiveinlining
    {
        ldarg.0
        sizeof [System.Runtime]System.UIntPtr
        sub
        ldc.i4.4
        sub
        ret
    }

    .method public hidebysig static !!T[] AsArray<T>(!!0& firstElement) cil managed aggressiveinlining
    {
        ldarg.0
        sizeof [System.Runtime]System.UIntPtr
        sub
        sizeof [System.Runtime]System.UIntPtr
        sub
        ret
    }

    .method public hidebysig static !!TTo As<TFrom, TTo>(!!TFrom from) cil managed aggressiveinlining
    {
        ldarg.0
        ret
    }

    .method public hidebysig static !!TTo& AsRef<TFrom, TTo>(!!TFrom from) cil managed aggressiveinlining
    {
        ldarg.0
        ret
    }

    .method public hidebysig static !!TTo RefAs<TFrom, TTo>(!!TFrom& from) cil managed aggressiveinlining
    {
        ldarg.0
        ret
    }

    .method public hidebysig static !!TTo* AsPointer<TFrom, TTo>(!!TFrom from) cil managed aggressiveinlining
    {
        ldarg.0
        ret
    }

    .method public hidebysig static !!TTo PointerAs<TFrom, TTo>(!!TFrom* from) cil managed aggressiveinlining
    {
        ldarg.0
        ret
    }
}
