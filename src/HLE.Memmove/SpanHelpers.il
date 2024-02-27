
.assembly extern System.Runtime
{
    .ver 9:0:0:0
    .publickeytoken = ( B0 3F 5F 7F 11 D5 0A 3A )
}

.assembly HLE.Memmove
{
    .ver 0:0:0:0
}

.module HLE.Memmove

.class public abstract auto ansi sealed HLE.Memmove.SpanHelpers extends [System.Runtime]System.Object
{
    .method public static void Memmove<T>(!!T& destination,!!T& source,native uint elementCount) cil managed aggressiveinlining
    {
          .maxstack 8
          ldarg.0
          ldarg.1
          ldarg.2
          call void [System.Runtime]System.Buffer::Memmove<[1]>(!!T&,!!T&,native uint)
          ret
    }
}
