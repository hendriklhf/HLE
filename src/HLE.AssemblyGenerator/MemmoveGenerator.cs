using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace HLE.AssemblyGenerator;

internal static class MemmoveGenerator
{
    private const string AssemblyName = "HLE.Memmove";
    private const string LibraryFileName = "HLE.Memmove.dll";
    private const string NamespaceName = "HLE.Memmove";
    private const string TypeName = "SpanHelpers";
    private const string MethodName = "Memmove";

    public static void Generate()
    {
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefinePersistedAssembly(new(AssemblyName), typeof(object).Assembly);

        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(AssemblyName);

        TypeBuilder typeBuilder = moduleBuilder.DefineType($"{NamespaceName}.{TypeName}", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

        MethodBuilder methodBuilder = typeBuilder.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);
        SetCustomMethodAttributes(methodBuilder);
        SetParameters(methodBuilder);
        GenerateMethodBody(methodBuilder.GetILGenerator());

        _ = typeBuilder.CreateType();
        assemblyBuilder.Save($@"..\..\{LibraryFileName}");
    }

    private static void SetParameters(MethodBuilder methodBuilder)
    {
        methodBuilder.DefineGenericParameters("T");
        Type genericType = Type.MakeGenericMethodParameter(0);
        Type genericByRefType = genericType.MakeByRefType();

        methodBuilder.SetReturnType(typeof(void));
        methodBuilder.SetParameters(genericByRefType, genericByRefType, typeof(nuint));

        methodBuilder.DefineParameter(1, ParameterAttributes.None, "destination");
        methodBuilder.DefineParameter(2, ParameterAttributes.None, "source");
        methodBuilder.DefineParameter(3, ParameterAttributes.None, "elementCount");
    }

    private static void SetCustomMethodAttributes(MethodBuilder methodBuilder)
    {
        ConstructorInfo methodImplAttributeConstructor = typeof(MethodImplAttribute).GetConstructor([typeof(MethodImplOptions)])!;
        CustomAttributeBuilder attributeBuilder = new(methodImplAttributeConstructor, [MethodImplOptions.AggressiveInlining]);
        methodBuilder.SetCustomAttribute(attributeBuilder);
    }

    private static void GenerateMethodBody(ILGenerator generator)
    {
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Ldarg_2);
        generator.EmitCall(OpCodes.Call, GetMemmoveMethodInfo(), null);
        generator.Emit(OpCodes.Ret);
    }

    private static MethodInfo GetMemmoveMethodInfo()
    {
        MethodInfo? memmove = Array.Find(
            typeof(Buffer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static),
            static m => m is { Name: "Memmove", IsGenericMethod: true }
        );

        if (memmove is not null)
        {
            return memmove;
        }

#if NET9_0_OR_GREATER
        memmove = Array.Find(
            Type.GetType("System.SpanHelpers")!.GetMethods(BindingFlags.NonPublic | BindingFlags.Static),
            static m => m is { Name: "Memmove", IsGenericMethod: true }
        );

        if (memmove is not null)
        {
            return memmove;
        }
#endif

        throw new InvalidOperationException("Could not find a suitable memmove function.");
    }
}
