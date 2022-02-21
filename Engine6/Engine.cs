namespace Engine;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Gl;
using Win32;
using System.Text;
using System.Numerics;

class Engine {
    //unsafe private static void TestRawDevices () {
    //    uint deviceCount = 0;
    //    _ = User.GetRawInputDeviceList(null, &deviceCount, (uint)RawInputDeviceList.Size);
    //    var devices = new RawInputDeviceList[deviceCount];
    //    fixed (RawInputDeviceList* p = devices) {
    //        var eh = User.GetRawInputDeviceList(p, &deviceCount, (uint)RawInputDeviceList.Size);
    //        if (eh != devices.Length)
    //            throw new Exception();
    //    }

    //    var devInfo = new RawInputDeviceInfo() { size = (uint)Marshal.SizeOf<RawInputDeviceInfo>() };
    //    var infoSize = devInfo.size;
    //    var charCount = 0u;
    //    var array = new ushort[1024];
    //    for (var i = 0; i < devices.Length; i++) {

    //        var ptr = devices[i].device;
    //        var x = User.GetRawInputDeviceInfoW(ptr, User.RawInputDeviceCommand.DeviceInfo, &devInfo, &infoSize);
    //        Debug.Assert(x > 0, $"{Kernel.GetLastError():x}");
    //        var type = devInfo.type;
    //        Console.WriteLine($"{i}: {type}, ");
    //        switch (type) {
    //            case RawInputDeviceType.Hid: {
    //                    DebugDump(devInfo.hid, MemberTypes.Field);
    //                }
    //                break;
    //            case RawInputDeviceType.Keyboard: {
    //                    DebugDump(devInfo.keyboard, MemberTypes.Field);
    //                }
    //                break;
    //            case RawInputDeviceType.Mouse: {
    //                    DebugDump(devInfo.mouse, MemberTypes.Field);
    //                }
    //                break;
    //        }

    //        var y = User.GetRawInputDeviceInfoW(ptr, User.RawInputDeviceCommand.DeviceName, null, &charCount);
    //        if (4l * charCount < 1024l) {

    //            fixed (ushort* ushorts = array)
    //                _ = User.GetRawInputDeviceInfoW(ptr, User.RawInputDeviceCommand.DeviceName, ushorts, &charCount);
    //            string name = TryGetString(array, (int)charCount);

    //            Console.WriteLine($"charCount: {charCount}, len: '{name}'");
    //        } else
    //            Console.WriteLine($"{charCount} ?!");
    //    }
    //}

    private static void DebugDump (object ob, MemberTypes types) {
        if (ob is null)
            return;
        var type = ob.GetType();
        Debug.WriteLine(type.Name);
        foreach (var m in type.GetMembers())
            if (types.HasFlag(m.MemberType))
                switch (m) {
                    case PropertyInfo pi:
                        if (pi.CanRead)
                            Debug.WriteLine($"<{pi.PropertyType.Name}> {pi.Name} : '{pi.GetValue(ob)}'");
                        break;
                    case FieldInfo fi:
                        Debug.WriteLine($"<{fi.FieldType.Name}> {fi.Name} : '{fi.GetValue(ob)}'");
                        break;
                }
    }
    private static string TryGetString (ushort[] span, int maxLength) {
        Debug.Assert(maxLength < 1024);
        var l = 0;
        while (l < maxLength && span[l] != 0)
            l++;
        if (l == 0)
            return "";
        Span<byte> bytes = stackalloc byte[l];
        for (var i = 0; i < l; i++)
            bytes[i] = (byte)span[i];
        return Encoding.ASCII.GetString(bytes);
    }
    static void Quaternions () {
        var x = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0);
        var y = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 2));
    }

    static int? ParseInt32 (string str) => int.TryParse(str, out var i) ? i : null;

    [STAThread]
    static void Main (string[] args) {

        var size = args.Length == 2 && ParseInt32(args[0]) is int w && ParseInt32(args[1]) is int h ? new(w, h) : new Vector2i(320, 240);
        using var gl = new BlitTest(size);
        gl.Run();
    }
}
