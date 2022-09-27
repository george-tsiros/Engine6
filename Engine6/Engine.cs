using Engine6;
//using System;
//using System.Text;
//using System.Diagnostics;
using Win32;

//unsafe {
//    var data = Win32.User32.GetMonitorInfo();
//    for (var i = 0; i < data.Length; i++) {
//        var datum = data[i];
//        var length = 0;
//        while (0 != datum.name[length])
//            ++length;
//        var name = 0 < length ? Encoding.ASCII.GetString(new Span<byte>(datum.name, length)) : string.Empty;
//        Debug.Write($"#{i}, '{name}': 0x{datum.flags:x8}, monitor {datum.monitor}, work {datum.work}\n");
//        if (Win32.User32.EnumDisplaySettings(datum.name, out var info)) {
//            Debug.Write($"{info.dmDisplayFrequency}\n");
//        } else
//            Debug.Write(":(");
//    }
//}
using (GdiWindow window = new() { BackgroundColor= Color.Red })
    window.Run();
using (GdiWindow window = new() { BackgroundColor= Color.Green })
    window.Run();
using (GdiWindow window = new() { BackgroundColor= Color.Blue })
    window.Run();
