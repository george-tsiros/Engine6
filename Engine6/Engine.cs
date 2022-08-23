namespace Engine6;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

unsafe class Engine6 {
    static ushort Atom;
    static BitmapInfo info = new();
    static IntPtr bitmap;
    static WndProc myWndProc, myWndProc2;
    static long lastTicks;
    const int X = 100, Y = 100, W = 640, H = 480;
    static IntPtr WindowHandle;
    static uint* pixels;
    static bool OnEdge (int x, int y, in BitmapInfoHeader h) =>
        x == 0 || y == 0 || x + 1 == h.width || y + 1 == h.height;

    static uint intensity = 0;
    static void Paint (IntPtr dc, in Rectangle r) {
        if (IntPtr.Zero == bitmap || null == pixels)
            return;
        int iw = info.header.width;
        int ih = info.header.height;
        Debug.Assert(W == iw);
        Debug.Assert(H == ih);
        var pixelCount = iw * ih;
        intensity = intensity < 255 ? intensity + 1 : 0;
        var value = 0xff000000 | (intensity << 16) | (intensity << 8) | intensity;
        for (var i = 0; i < pixelCount; ++i)
            pixels[i] = value;
        //for (int y = 0; y < ih; ++y)
        //    for (int x = 0; x < iw; ++x)
        //        pixels[y * iw + x] = OnEdge(x, y, info.header) ? 0xff000000 : v;
        int rw = r.Width;
        int rh = r.Height;
        Debug.Assert(W == rw);
        Debug.Assert(H == rh);

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
    static string creationStage = null;

    static nint MyWndProc2 (IntPtr hWnd, WinMessage m, nuint w, nint l) {
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
                return 0;
        }
        return User32.DefWindowProcA(hWnd, m, w, l);
    }
    static nint MyWndProc (IntPtr hWnd, WinMessage m, nuint w, nint l) {
        if (creationStage is not null)
            Debug.WriteLine($"{creationStage}: {m}");
        switch (m) {
            case WinMessage.Close:
                User32.PostQuitMessage(0);
                return 0;
            case WinMessage.KeyDown:
                if (KeyDown((byte)(w & 0xff))) {
                    return 0;
                }
                break;
                //case WinMessage.Paint:
                //PaintStruct ps = new();
                //var dc = User32.BeginPaint(hWnd, ref ps);
                //if (!ps.rect.IsEmpty)
                //    Paint(dc, ps.rect);
                //User32.EndPaint(hWnd, ref ps);
                //lastTicks = Stopwatch.GetTimestamp();
                //return 0;
        }
        return User32.DefWindowProcA(hWnd, m, w, l);
    }


    static int Loop () {
        long T = Stopwatch.Frequency;
        long frameDelay = T / 36;
        Message m = new();
        for (; ; ) {
            while (User32.PeekMessageA(ref m, IntPtr.Zero, 0, 0, PeekRemove.NoRemove)) {
                var gotMessage = User32.GetMessageA(ref m, IntPtr.Zero, 0, 0);
                if (gotMessage == 0)
                    return 0;
                if (gotMessage == -1)
                    return 3;
                _ = User32.DispatchMessageA(ref m);
            }

            var t0 = Stopwatch.GetTimestamp();
            if (lastTicks + frameDelay < t0) {
                lastTicks = t0 + frameDelay;
                User32.InvalidateWindow(WindowHandle);
            }
        }
    }

    static void Main () {
        new GdiWindow().Run();
    }
    static void notMain () {
        myWndProc = new(MyWndProc);
        myWndProc2 = new(MyWndProc2);

        var wc = new WindowClassExA();
        wc.style = ClassStyle.VRedraw | ClassStyle.HRedraw;
        wc.wndProc = myWndProc;
        wc.classname = "AAAA";
        creationStage = "RegisterClass";
        Atom = User32.RegisterClass(ref wc);

        var module = Kernel32.GetModuleHandleA(null);
        creationStage = "CreateWindow";
        WindowHandle = User32.CreateWindow(Atom, W, H, module, WindowStyle.Visible);
        Debug.Assert(IntPtr.Zero != WindowHandle);

        creationStage = "SetWindow";
        User32.SetWindow(WindowHandle, WindowStyle.Overlapped);
        creationStage = "UpdateWindow";
        User32.UpdateWindow(WindowHandle);
        creationStage = "ShowWindow";
        _ = User32.ShowWindow(WindowHandle, CmdShow.ShowNormal);
        creationStage = null;
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
        User32.SetWindow(WindowHandle, myWndProc2);
        _ = Loop();
    }
}

