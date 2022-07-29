namespace Engine;

using System;
using System.Numerics;
using Shaders;
using System.Diagnostics;
using Gl;
using Win32;
using static Gl.Opengl;
using static Gl.Utilities;
using System.Threading;

class BlitTest:GlWindowArb {
    const int TicksPerSecond = 10_000_000;
    const int TicksPerMillisecond = TicksPerSecond / 1000;
    const int TicksPerMicrosecond = TicksPerMillisecond / 1000;
    const int PerfWidth = 300;
    const int FrequencyLog10 = 7;
    const int MaxParallelism = 16;
    const int MinFramerate = 30;
    const int MaxFramerate = 144;
    static readonly Vector2i Eight = new(8, 8), Sixteen = new(16, 16);

    static void Log (object ob) =>
#if DEBUG
        Debug
#else
        Console
#endif
        .WriteLine(ob);

    public BlitTest (Vector2i size, Model m = null) : base(size) {
        Debug.Assert(Stopwatch.Frequency == 10_000_000);
        Text = "asdfg";
        const string TeapotFilepath = @"data\teapot.obj";
        model = m ?? new Model(TeapotFilepath);
        PointCount = model.Vertices.Count;
        Log(PointCount);
        points3D = model.Vertices.ToArray();
        points2Di = new Vector2i[PointCount];
        NormalizedPoints3D = new Vector3[PointCount];
        DepthBuffer = new float[Width * Height];
        Radius = double.Min(Width, Height) / 4;
        Center = new Vector2i(Width / 2, Height / 2);
        State.SwapInterval = 0;
        RecalculateTimings();
    }
    readonly double Radius;
    readonly Vector2i Center;

    Raster raster;
    Sampler2D sampler;
    VertexArray quad;
    VertexBuffer<Vector4> quadBuffer;
    float[] DepthBuffer;
    int PointCount;
    Vector3[] points3D;
    Vector2i[] points2Di;
    Vector3[] NormalizedPoints3D;

    float theta, phi;

    Matrix4x4 viewProjection;
    readonly Matrix4x4 ModelTranslation = Matrix4x4.Identity;
    bool leftDown;

    Framebuffer fb;
    Sampler2D fbsampler;
    Renderbuffer rb;
    Font font;
    Model model;
    int desiredFrameRate = MaxFramerate;
    protected override void Load () {

        font = new("data\\Kepler452b-Mono.txt");
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, (float)Width / Height, 1, 100);
        var view = Matrix4x4.CreateLookAt(new(0, 0, float.Max(model.Max.Length(), model.Min.Length()) + 5), -Vector3.UnitZ, Vector3.UnitY);
        viewProjection = view * projection;

        rb = new(Size, RenderbufferFormat.Depth24Stencil8);

        fbsampler = new(Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };

        fb = new();

        fb.Attach(rb, FramebufferAttachment.DepthStencil);

        fb.Attach(fbsampler, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(fb, DrawBuffer.Color0);

        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        raster = new(new(Width, Height), 4, 1);

        sampler = new(raster.Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        Disposables.Add(raster);
        Disposables.Add(sampler);
        Disposables.Add(quad);
        Disposables.Add(quadBuffer);
        CursorVisible = false;
    }

    protected override void ButtonDown (int b) {
        if (b == 1)
            leftDown = true;
    }
    protected override void ButtonUp (int b) {
        if (b == 1)
            leftDown = false;
    }
    Vector2i previousCursorLocation = -Vector2i.UnitX;
    protected override void MouseMove (Vector2i location) {
        if (!leftDown)
            return;
        if (0 <= previousCursorLocation.X) {
            var (dx, dy) = location - previousCursorLocation;
            theta += 0.001f * dx;
            phi += 0.001f * dy;
            if (theta > float.Tau)
                theta -= float.Tau;
            else if (theta < 0)
                theta += float.Tau;
            if (phi > float.Tau)
                phi -= float.Tau;
            else if (phi < 0)
                phi += float.Tau;
        }
        previousCursorLocation = location;
    }

    const int HistoryDepth = 256;
    private readonly long[] Deltas = new long[HistoryDepth];
    private readonly double[] Means = new double[HistoryDepth];
    private int meanIndex = 0;
    const int BinCount = 100;
    private readonly int[] bins = new int[BinCount];
    private int syncIndex = 0;
    int targetFramerate;//= State.SwapInterval == -1 ? desiredFrameRate : MaxFramerate;
    int ticksPerFrame;//= (double)Stopwatch.Frequency / targetFramerate;
    long previousSyncTime;
    //static float Frac (float f) => f - float.Floor(f);
    static double Frac (double d) => d - double.Floor(d);
    int adjustment = 1000;
    long lastDelta;
    long sum;
    protected override void Render () {
        var textRow = -font.Height;
        var mvp = Matrix4x4.CreateRotationY(theta) * Matrix4x4.CreateRotationX(phi) * ModelTranslation * viewProjection;
        MemSet(raster.Pixels, Color.Black);
        //Array.Clear(DepthBuffer);
        //for (int i = 0; i < PointCount; i++) {
        //    var w = Vector4.Transform(points3D[i], mvp);
        //    Debug.Assert(w.W > 1e-10);
        //    var (x, y) = (w.X / w.W, w.Y / w.W);
        //    var p = new Vector3(x, y, w.Z / w.W);
        //    NormalizedPoints3D[i] = p;
        //    var xy = new Vector2(x, y);
        //    points2Di[i] = new Vector2i((int)float.Round(0.5f * (x + 1) * Width), (int)float.Round(.5f * (y + 1) * Height));
        //}
        //foreach (var f in model.Faces) {
        //    var (a, b, c) = points3D.Dex(f);
        //}
        var originRow = Height / 4 * 3;
        var x0 = (Width - HistoryDepth) / 2;
        raster.HorizontalU32(x0, originRow + 1, HistoryDepth, Color.Red);
        raster.HorizontalU32(x0, originRow - BinCount - 1, HistoryDepth, Color.Red);
        raster.DrawString(syncs[1 + State.SwapInterval], font, 0, textRow += font.Height);
        if (-1 == State.SwapInterval)
            raster.DrawString($"target: {targetFramerate}", font, 0, textRow += font.Height);

        //raster.VerticalU32(Width / 2, 0, Height);
        var delta = LastSync - previousSyncTime;
        sum += delta;
        Deltas[syncIndex] = delta;
        previousSyncTime = LastSync;

        if (++syncIndex == HistoryDepth)
            syncIndex = 0;

        if (HistoryDepth < FramesRendered) {
            Array.Clear(bins);
            sum -= lastDelta;
            lastDelta = delta;

            var meanTicks = (int)double.Round((double)sum / HistoryDepth);
            raster.DrawString($"meanticks: {meanTicks}", font, 0, textRow += font.Height);

            var meanFps = (long)double.Round((double)TicksPerSecond / meanTicks);

            foreach (var dt in Deltas) {
                var r = (double)(dt % meanTicks) / meanTicks;
                var bin = (int)double.Floor(BinCount * r);
                ++bins[bin];
            }

            raster.DrawString($"actual: {double.Round(meanFps, 1)}", font, 0, textRow += font.Height);
            Means[meanIndex] = meanFps;
            if (++meanIndex == HistoryDepth)
                meanIndex = 0;
            if (meanFps >= 30) {
                var framerateIsStable = true;
                for (var i = 0; i < HistoryDepth && framerateIsStable; ++i)
                    framerateIsStable = double.Abs(Means[i] - meanFps) / meanFps < .01;

                if (framerateIsStable) {
                    var diff = meanFps - targetFramerate;
                    var changed = true;
                    if (diff < -.1)
                        ++adjustment;
                    else if (.1 < diff)
                        --adjustment;
                    else
                        changed = false;
                    if (changed) {
                        raster.DrawString($"diff: {(diff < 0 ? '-' : '+')} {double.Round(double.Abs(diff / targetFramerate * 100), 2),3} %", font, 0, textRow += font.Height, Color.Yellow);
                        raster.DrawString($"adjustment: {adjustment}", font, 0, textRow += font.Height, Color.Yellow);
                    } else
                        raster.DrawString($"stable", font, 0, textRow += font.Height, Color.Green);
                } else
                    raster.DrawString($"unstable", font, 0, textRow += font.Height, Color.Red);
            }

            for (var i = 0; i < BinCount; ++i) {
                var density = bins[i];
                //var color = ~0x00ffffffu | (intensity << 16) | (intensity << 8) | intensity;
                raster.HorizontalU32((Width - density) / 2, originRow - i, density, Color.White);
            }
        }

        //for (var i = 0; i < HistoryDepth; ++i) {
        //    var x = lastSyncIndex + i;
        //    if (HistoryDepth <= x)
        //        x -= HistoryDepth;
        //    var intensity = 5 * i;
        //    var color = ~0x00ffffff | (intensity << 16) | (intensity << 8) | intensity;
        //    var t = AllLastSyncs[x];
        //    var (sin, cos) = double.SinCos(double.Tau * (t % ticksPerFrame) / ticksPerFrame);
        //    raster.FillRectU32(new(Vector2d.Round(Center - Eight + Radius * new Vector2d(cos, sin)), Sixteen), (uint)color);
        //}
        var (cx, cy) = CursorLocation;
        if (0 <= cx && cx < Width && 0 <= cy && cy < Height) {
            raster.HorizontalU32(cx, cy, 10, Color.Green);
            raster.VerticalU32(cx, cy, 10, Color.Green);
        }
        //PutPixels(raster, points2Di, Color.White);
        //var fi = FrameIndex == 0 ? FrameTicks.Length - 1 : FrameIndex - 1;
        //var ft = FrameTicks[fi];
        //if (ft > 0) {
        //    var l = (int)double.Round(double.Log10(double.Clamp(ft, 1, SecondTicks)) * PerfWidth / FrequencyLog10);
        //    var length = int.Clamp(l, 0, PerfWidth);
        //    var r = (int)float.Floor(255f * length / PerfWidth);
        //    var g = (int)float.Floor(255f * (PerfWidth - length) / PerfWidth);
        //    uint color = 0xff000000u | (uint)(r << 16) | (uint)(g << 8);
        //    if (length > 0)
        //        Horizontal(raster, 0, fi, length, color);
        //    Horizontal(raster, length, fi, PerfWidth - length, Color.QuarterBlue);
        //}

        sampler.Upload(raster);
        State.Framebuffer = fb;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Always;
        State.CullFace = true;
        sampler.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);

        State.Framebuffer = 0;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);

        fbsampler.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);
        if (-1 == State.SwapInterval) {
            var nextSyncTicks = LastSync + ticksPerFrame - adjustment;
            while (Stopwatch.GetTimestamp() < nextSyncTicks)
                ;
        }
    }


    public unsafe static void PutPixels (Raster raster, Vector2i[] points, uint color = ~0u) {
        Debug.Assert(raster.Stride == raster.Width * 4);
        fixed (byte* bytes = raster.Pixels) {
            uint* p = (uint*)bytes;
            foreach (var point in points)
                if (0 <= point.X && 0 <= point.Y && point.X < raster.Width && point.Y < raster.Height)
                    p[raster.Width * point.Y + point.X] = color;
        }
    }
    static readonly string[] syncs = "Free sink,No VSync at all,\"Common\" VSync".Split(',');
    protected override void KeyUp (Keys k) {
        base.KeyUp(k);
    }
    void RecalculateTimings () {
        targetFramerate = State.SwapInterval == -1 ? desiredFrameRate : MaxFramerate;
        ticksPerFrame = TicksPerSecond / targetFramerate;
    }
    protected override void KeyDown (Keys k) {
        switch (k) {
            case Keys.Space:
                var s = State.SwapInterval + 1;
                if (s > 1)
                    s = -1;
                State.SwapInterval = s;
                break;
            case Keys.Up:
                if (State.SwapInterval == -1)
                    desiredFrameRate = int.Clamp(desiredFrameRate + 1, MinFramerate, MaxFramerate);
                break;
            case Keys.Down:
                if (State.SwapInterval == -1)
                    desiredFrameRate = int.Clamp(desiredFrameRate - 1, MinFramerate, MaxFramerate);
                break;
            case Keys.PageUp:
                if (State.SwapInterval == -1)
                    desiredFrameRate = int.Clamp(desiredFrameRate + 10, MinFramerate, MaxFramerate);
                break;
            case Keys.PageDown:
                if (State.SwapInterval == -1)
                    desiredFrameRate = int.Clamp(desiredFrameRate - 10, MinFramerate, MaxFramerate);
                break;
            default:
                base.KeyDown(k);
                return;
        }
        RecalculateTimings();
    }
}
