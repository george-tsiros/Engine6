namespace Engine6;
using System;
using System.Text;
using System.Runtime.InteropServices;
using Gl;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

class Engine6 {
    public static bool IsNullable (Type t) => t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
    public static Type TryGetNullableType (Type t) => IsNullable(t) ? t.GetGenericArguments()[0] : null;
    public static string UnmangledGenericName (Type t) =>
        t.IsConstructedGenericType ?
        t.Name.Remove(t.Name.IndexOf('`')) :
        throw new ArgumentException("not a constructed generic type", nameof(t));
    private static readonly Dictionary<TypeCode, string> CSharpTypenames = new() {
        { TypeCode.Boolean, "bool" },
        { TypeCode.Byte, "byte" },
        { TypeCode.Decimal, "decimal" },
        { TypeCode.Double, "double" },
        { TypeCode.Int16, "short" },
        { TypeCode.Int32, "int" },
        { TypeCode.Int64, "long" },
        { TypeCode.SByte, "sbyte" },
        { TypeCode.Single, "float" },
        { TypeCode.String, "string" },
        { TypeCode.UInt16, "ushort" },
        { TypeCode.UInt32, "uint" },
        { TypeCode.UInt64, "ulong" },
    };


    public static string NameOf (Type type) {

        if (type == typeof(object))
            return "object";

        if (type == typeof(void))
            return "void";

        if (type == typeof(nint))
            return "nint";

        if (type == typeof(nuint))
            return "nuint";

        if (type.IsArray && type.GetElementType() is Type elementType)
            return NameOf(elementType) + "[]";

        if (TryGetNullableType(type) is Type n)
            return NameOf(n) + "?";

        if (type.IsConstructedGenericType && type.GetGenericArguments() is Type[] typeArgs)
            return $"{UnmangledGenericName(type)}<{string.Join(", ", Array.ConvertAll(typeArgs, NameOf))}>";

        if (type.IsEnum)
            return type.Name;

        if (type.IsPointer)
            return NameOf(type.GetElementType()) + "*";

        return CSharpTypenames.TryGetValue(Type.GetTypeCode(type), out var simplename) ? simplename : type.Name;
    }


    internal const byte END = 0x00;
    internal const byte VOID = 0x00;
    internal const byte BYTE = 0x01;
    internal const byte SHORT = 0x02;
    internal const byte INT = 0x03;
    internal const byte LONG = 0x04;
    internal const byte SINGLE = 0x05;
    internal const byte DOUBLE = 0x06;
    internal const byte UNSIGNED = 0x40;
    // 0x7f + 1 max levels of indirection  (0x80 already means 1 level of indirection, that is, basic pointer, so 0xff = 0x80 + 0x7f means (0x7f+1) levels of indirection)
    internal const byte PTR = 0x80;
    static byte[] CreateSignature (Type returnType, string name, Type[] parameterTypes) {
        var charCount = Encoding.ASCII.GetByteCount(name);
        // trust nobody
        Debug.Assert(0 < charCount);
        Debug.Assert(name.Length == charCount);
        Debug.Assert(charCount < 256);
        var parameterBytes = 0;
        foreach (var type in parameterTypes)
            parameterBytes += type.IsPointer ? 2 : 1;
        var returnTypeBytes = returnType.IsPointer ? 2 : 1;
        var byteCount = 1 + charCount + returnTypeBytes + parameterBytes + 1;
        var bytes = new byte[byteCount];
        bytes[0] = (byte)charCount;
        var written = Encoding.ASCII.GetBytes(name, 0, charCount, bytes, 1);
        Debug.Assert(written == charCount);
        var i = 1 + charCount;
        AppendTypeInfo(bytes, returnType, ref i);
        foreach (var type in parameterTypes)
            AppendTypeInfo(bytes, type, ref i);
        Debug.Assert(i + 1 == byteCount);
        bytes[i] = END;
        return bytes;
    }
    static void AppendTypeInfo (byte[] bytes, Type type, ref int index) {
        var indirectionCount = 0;
        while (type.IsPointer) {
            ++indirectionCount;
            type = type.GetElementType();
        }
        if (0 < indirectionCount) {
            Debug.Assert(indirectionCount <= 0x80);
            bytes[index++] = (byte)(0x80 + indirectionCount - 1);
        }
        bytes[index++] = GetTypeCode(type);
    }

    static byte GetTypeCode (Type type) {
        if (type == typeof(long) || type == typeof(nint) || type == typeof(nuint))
            return LONG;
        if (type == typeof(void))
            return VOID;
        if (type == typeof(byte))
            return UNSIGNED + BYTE;
        if (type == typeof(sbyte))
            return BYTE;
        if (type == typeof(short))
            return SHORT;
        if (type == typeof(ushort))
            return UNSIGNED + SHORT;
        if (type == typeof(int) || type == typeof(bool))
            return INT;
        if (type == typeof(uint))
            return UNSIGNED + INT;
        if (type == typeof(ulong))
            return UNSIGNED + LONG;
        if (type == typeof(float))
            return SINGLE;
        if (type == typeof(double))
            return DOUBLE;
        throw new NotSupportedException($"{NameOf(type)} not supported");
    }
    static byte[] AsciiChars (string str) {
        var bytes = Encoding.ASCII.GetBytes(str);
        Debug.Assert(bytes.Length == str.Length);
        return bytes;
    }
    static void Main () {
        var nintType = typeof(nint);
        var nuintType = typeof(nuint);
        const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
        const BindingFlags AllStatic = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
        foreach (var method in typeof(Win32.User32).GetMethods(AllStatic)) {
            if (method.GetCustomAttribute<DllImportAttribute>() is DllImportAttribute imp) {
                var libraryName = imp.Value;
                var name = imp.EntryPoint is string _0 && !string.IsNullOrEmpty(_0) ? _0 : method.Name;

                var parameterTypes = Array.ConvertAll((ParameterInfo[])method.GetParameters(), p => p.ParameterType);
                if (Array.Exists(parameterTypes, type => type.IsByRef))
                    continue;
                try {
                    var bytes = CreateSignature(method.ReturnType, name, parameterTypes);
                    Debug.Write($"{name}: {string.Join(", ", Array.ConvertAll(bytes, b => b.ToString("x2")))}\n");
                } catch (NotSupportedException e) {
                    Debug.Write($"failed for {name} because {e.Message}\n");
                }
            }
        }
        ContextConfiguration c = new() {
            ColorBits = 32,
            DepthBits = 24,
            DoubleBuffer = true,
            Profile = ProfileMask.Core,
            SwapMethod = SwapMethod.Swap,
            Flags = ContextFlag.Debug | ContextFlag.ForwardCompatible,
            Version = new(4, 5),
        };
        using (var f = new CubeTest(c))
            f.Run();
    }
}
