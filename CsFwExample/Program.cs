namespace CsFwExample;

using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

struct RawInputDevice {
    public ushort usagePage, usage;
    public RegisterInput flags;
    public int windowHandle;
}

[Flags]
enum RegisterInput:ulong {
    None = 0,
    Remove = 1 << 0,
    Exclude = 1 << 4,
    PageOnly = 1 << 5,
    NoLegacy = Exclude | PageOnly,
    InputSink = 1 << 8,
    CaptureMouse = 1 << 9,
    NoHotKeys = 1 << 9,
    AppKeys = 1 << 10,
}

class Program:Form {

    private readonly int count;
    private readonly List<TimePoint> events;
    private readonly Point[] points;
    private readonly long timerFrequency;
    private TrackBar slider;
    private Program () {
        MaximizeBox = false;
        ClientSize = new Size(1000, 1000);
        var b = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(b))
            g.Clear(Color.Black);
        BackgroundImage = b;
        BackgroundImageLayout = ImageLayout.Center;
        Debug.Assert(ReferenceEquals(b, BackgroundImage));
        FormBorderStyle = FormBorderStyle.FixedSingle;
        events = new List<TimePoint>(TimePoint.FromFile("movements.bin"));

        using (var f = File.OpenRead("movements.bin")) {
            var bytes = new byte[sizeof(long)];
            var read = f.Read(bytes, 0, sizeof(long));
            if (read != sizeof(long))
                throw new ApplicationException();
            timerFrequency = BitConverter.ToInt64(bytes, 0);
        }

        count = events.Count;
        var boundingRect = BoundingRectangle(events);

        points = new Point[count];
        for (var i = 0; i < count; i++)
            points[i] = events[i].p - (Size)boundingRect.Location;
    }

    private static Rectangle BoundingRectangle (IEnumerable<TimePoint> points) {
        var (minx, maxx, miny, maxy) = (int.MaxValue, int.MinValue, int.MaxValue, int.MinValue);
        foreach (var d in points) {
            minx = Math.Min(minx, d.p.X);
            miny = Math.Min(miny, d.p.Y);
            maxx = Math.Max(maxx, d.p.X);
            maxy = Math.Max(maxy, d.p.Y);
        }
        return new Rectangle(minx, miny, maxx - minx, maxy - miny);
    }

    protected override void OnLoad (EventArgs _) {
        slider = new TrackBar();
        slider.Location = new Point(slider.Margin.Left, slider.Margin.Top);
        slider.Width = ClientSize.Width - slider.Margin.Horizontal;
        slider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        slider.Minimum = 0;
        slider.Maximum = count - 1;
        //Controls.Add(slider);
        Invalidate();
    }
    unsafe protected override void OnPaintBackground (PaintEventArgs args) {
        using (var g = Graphics.FromImage(BackgroundImage))
            g.Clear(Color.Black);
        var b = (Bitmap)BackgroundImage;
        var l = b.LockBits(ImageLockMode.WriteOnly);
        var p = (uint*)l.Scan0.ToPointer();
        var w = l.Width;
        foreach (var pt in points)
            if (0 <= pt.Y && pt.Y < l.Height && 0 <= pt.X && pt.X < w)
                p[w * pt.Y + pt.X] = ~0u; 
        b.UnlockBits(l);
        base.OnPaintBackground(args);
    }
    unsafe private static void Proc () {
        const int capacity = 10000;
        var events = new TimePoint[capacity];

        var previousLocation = CursorPosition.Get();
        var index = 0;
        var t0 = Stopwatch.GetTimestamp();
        var factor = 1000.0 / Stopwatch.Frequency;
        while (run && index < capacity) {
            var t = Stopwatch.GetTimestamp();
            var currentLocation = CursorPosition.Get();
            if (previousLocation != currentLocation) {
                events[index++] = new TimePoint() { millis = factor * (t - t0), p = currentLocation };
                previousLocation = currentLocation;
            }
        }
        using (var writer = new BinaryWriter(File.Create("movements.bin"))) {
            foreach (var e in events) {
                if (e.millis == 0)
                    break;
                writer.Write(e.millis);
                writer.Write(e.p.X);
                writer.Write(e.p.Y);
            }
        }
    }

    private volatile static bool run = true;
    private static void Main () {
        //var thread = new Thread(Proc);
        //thread.Start();
        //_ = Console.ReadLine();
        //run = false;
        //thread.Join();
        Application.Run(new Program());
    }
}
