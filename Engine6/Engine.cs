namespace Engine6;
using Win32;
using System.Threading;
class Engine6 {
    
    static void Main () {
        using GdiWindow window = new();
        window.Run();
    }
}