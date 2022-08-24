using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

namespace Engine6;

class Engine6 {
    static void Main () {
        Debug.Assert(80 == Marshal.SizeOf<CreateStructA>());
        new GdiWindow().Run();
    }
}

