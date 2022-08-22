namespace Engine6;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

unsafe class Engine6 {
    static ushort Atom;
    static BitmapInfo info = new();
    static IntPtr bitmap;
    static WndProc myWndProc;
    static long lastTicks;
    const int X = 100, Y = 100, W = 320, H = 240;
    static IntPtr WindowHandle;
    static uint* pixels;
    static bool OnEdge (int x, int y, in BitmapInfoHeader h) =>
        x == 0 || y == 0 || x + 1 == h.width || y + 1 == h.height;

    static void Paint (IntPtr dc, in Rectangle r) {
        if (IntPtr.Zero == bitmap || null == pixels)
            return;
        int iw = info.header.width;
        int ih = info.header.height;
        Debug.Assert(320 == iw);
        Debug.Assert(240 == ih);
        uint v = (uint)(Stopwatch.GetTimestamp() & 0xffffffu);

        for (int y = 0; y < ih; ++y)
            for (int x = 0; x < iw; ++x)
                pixels[y * iw + x] = OnEdge(x, y, info.header) ? 0xff000000 : v;
        int rw = r.Width;
        int rh = r.Height;
        Debug.Assert(320 == rw);
        Debug.Assert(240 == rh);

        fixed (BitmapInfo* bip = &info) {
            int lines = Gdi32.StretchDIBits(dc, 0, 0, rw, rh, 0, 0, iw, ih, pixels, bip, 0, 0xcc0020);
            Debug.Assert(0 != lines);
        }
    }
    static bool KeyDown (byte b) {
        switch ((Keys)b) {
            case Keys.Escape:
                User32.PostQuitMessage(0);
                return true;
        }
        return false;
    }
    static nint MyWndProc (IntPtr hWnd, WinMessage m, nuint w, nint l) {
        switch (m) {
            case WinMessage.Close:
                User32.PostQuitMessage(0);
                return 0;
            case WinMessage.KeyDown:
                if (KeyDown((byte)(w & 0xff))) {
                    return 0;
                }
                break;
            case WinMessage.Paint:
                PaintStruct ps = new();
                var dc = User32.BeginPaint(hWnd, ref ps);
                if (!ps.rect.IsEmpty)
                    Paint(dc, ps.rect);
                User32.EndPaint(hWnd, ref ps);
                lastTicks = Stopwatch.GetTimestamp();
                return 0;
        }
        return User32.DefWindowProcA(hWnd, m, w, l);
    }
    static int Loop () {
        long T = Stopwatch.Frequency;
        long frameDelay = T / 10;
        Message m = new();
        for (; ; ) {
            while (User32.PeekMessageA(ref m, IntPtr.Zero, 0, 0, PeekRemove.NoRemove)) {
                var gotMessage = User32.GetMessageA(ref m, IntPtr.Zero, 0, 0);
                if (gotMessage == 0)
                    return 0;
                if (gotMessage == -1)
                    return 3;
                User32.DispatchMessageA(ref m);
            }
            if (lastTicks + frameDelay < Stopwatch.GetTimestamp()) {
                User32.InvalidateWindow(WindowHandle);
            }
        }

    }

    static void Main () {
        myWndProc = new(MyWndProc);

        var wc = new WindowClassExA();
        wc.style = ClassStyle.VRedraw | ClassStyle.HRedraw;
        wc.wndProc = myWndProc;
        wc.classname = "AAAA";
        Atom = User32.RegisterClass(ref wc);

        var module = Kernel32.GetModuleHandleA(null);
        WindowHandle = User32.CreateWindow(Atom, W,H,module, WindowStyle.Visible);
        Debug.Assert(IntPtr.Zero != WindowHandle);

        User32.SetWindow(WindowHandle, WindowStyle.Overlapped);
        User32.UpdateWindow(WindowHandle);
        User32.ShowWindow(WindowHandle, CmdShow.ShowNormal);

        info.header.size = (uint)Marshal.SizeOf<BitmapInfoHeader>();
        info.header.width = W;
        info.header.height = H;
        info.header.planes = 1;
        info.header.bitCount = BitCount.ColorBits32;
        info.header.compression = BitmapCompression.Rgb;
        
        var dc = new DeviceContext(WindowHandle);
        Debug.Assert(IntPtr.Zero != (IntPtr)dc);
        var errorBefore = Kernel32.GetLastError();
        if (0 != errorBefore)
            throw new WinApiException("??", errorBefore);
        void* p = null;
        bitmap = Gdi32.CreateDIBSection(dc, ref info, ref p);
        Debug.Assert(IntPtr.Zero != bitmap);
        Debug.Assert(null != p);
        pixels = (uint*)p;
        dc.Close();
        _ = Loop();
    }
}

