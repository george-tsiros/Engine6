namespace Gl;

using System;
using System.Diagnostics;
using Win32;

public abstract class WindowBase:IDisposable {
    private bool disposed;
    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            //_ = Demand(CaptureMouse(false));
            Closing();
            Demand(User.DestroyWindow(WindowHandle));
            Demand(User.UnregisterClassW(new IntPtr(ClassAtom), IntPtr.Zero));
        }
    }

    protected readonly WndProc wndProc;
    protected int Width { get; init; }
    protected int Height { get; init; }
    protected ushort ClassAtom { get; }
    protected IntPtr WindowHandle { get; init; }
    protected static readonly IntPtr SelfHandle = Kernel.GetModuleHandleW(null);
    protected virtual string ClassName => "MYWINDOWCLASS";

    public WindowBase (Vector2i size) {
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        wndProc = new(WndProc);
        ClassAtom = User.RegisterWindowClass(wndProc, ClassName);
        WindowHandle = User.CreateWindow(ClassAtom, size, SelfHandle);
        (Width, Height) = size;
    }

    abstract protected IntPtr WndProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l);
    protected abstract void Closing ();

    public static IntPtr Demand (IntPtr p) {
        Demand(IntPtr.Zero != p);
        return p;
    }

    public static int Demand (int p) {
        Demand(0 != p);
        return p;
    }

    public static void Demand (bool condition, string message = null) {
        if (!condition) {
            var stackFrame = new StackFrame(1, true);
            var m = $">{stackFrame.GetFileName()}({stackFrame.GetFileLineNumber()},{stackFrame.GetFileColumnNumber()}): {message ?? "?"} ({Kernel.GetLastError():X})";
            if (Debugger.IsAttached)
                throw new Exception(m);
            else
                Console.WriteLine(m);
        }
    }
}
//class PipeClient {
//    static void NotMain (string[] args) {
//        if (args.Length == 0)
//            return;

//        using PipeStream pipeClient = new AnonymousPipeClientStream(PipeDirection.In, args[0]);
//        Console.WriteLine("[CLIENT] Current TransmissionMode: {0}.", pipeClient.TransmissionMode);

//        using var sr = new StreamReader(pipeClient);
//        string temp;

//        // Wait for 'sync message' from the server.
//        do {
//            Console.WriteLine("[CLIENT] Wait for sync...");
//            temp = sr.ReadLine();
//        }
//        while (!temp.StartsWith("SYNC"));

//        // Read the server data and echo to the console.
//        while ((temp = sr.ReadLine()) is not null)
//            Console.WriteLine("[CLIENT] Echo: " + temp);
//    }
//}
//class PipeServer {
//    static void NotMain () {
//        var pipeClient = new Process();

//        pipeClient.StartInfo.FileName = "pipeClient.exe";

//        using (var pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable)) {
//            Console.WriteLine("[SERVER] Current TransmissionMode: {0}.", pipeServer.TransmissionMode);

//            // Pass the client process a handle to the server.
//            pipeClient.StartInfo.Arguments = pipeServer.GetClientHandleAsString();
//            pipeClient.Start();

//            pipeServer.DisposeLocalCopyOfClientHandle();

//            // Read user input and send that to the client process.
//            try {
//                using StreamWriter sw = new StreamWriter(pipeServer);
//                sw.AutoFlush = true;

//                // Send a 'sync message' and wait for client to receive it.
//                sw.WriteLine("SYNC");
//                pipeServer.WaitForPipeDrain();

//                // Send the console input to the client process.
//                Console.Write("[SERVER] Enter text: ");
//                sw.WriteLine(Console.ReadLine());
//            }
//            // Catch the IOException that is raised if the pipe is broken
//            // or disconnected.
//            catch (IOException e) {
//                Console.WriteLine("[SERVER] Error: {0}", e.Message);
//            }
//        }

//        pipeClient.WaitForExit();
//        pipeClient.Close();
//        Console.WriteLine("[SERVER] Client quit. Server terminating.");
//    }
//}
