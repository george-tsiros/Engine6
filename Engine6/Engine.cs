using Engine6;
using System.Diagnostics;
unsafe {
    var data = Win32.User32.GetMonitorInfo();
    using GlWindow window = new();
    window.Run();
}