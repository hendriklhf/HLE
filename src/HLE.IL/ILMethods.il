
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

.class public abstract auto ansi sealed HLE.IL.ILMethods
{
    .method public static !!0& GetField<T>(object obj, native unsigned int byteOffset) cil managed aggressiveinlining
    {
        ldarg.0
        ldarg.1
        add
        ret
    }

    .method public static char& GetStringReference(string str) cil managed aggressiveinlining
    {
        ldarg.0
        sizeof [System.Runtime]System.UIntPtr
        add
        ldc.i4.4
        add
        ret
    }

    .method public static string AsString(char& chars) cil managed aggressiveinlining
    {
        ldarg.0
        sizeof [System.Runtime]System.UIntPtr
        sub
        ldc.i4.4
        sub
        ret
    }
}
