namespace Engine6;

using System.Threading;
using Gl;
using static Gl.Opengl;
using Shaders;
using System.Numerics;
using System;
using System.IO;
using System.Diagnostics;
using Win32;
using static Common.Maths;
using Common;
using System.Text;

internal class NoiseTest:GlWindowArb {
    private const int _WIDTH = 128, _HEIGHT = _WIDTH;
    private const float _XSCALE = 1000f / _WIDTH, _YSCALE = 1000f / _HEIGHT;
    private const int ThreadCount = 4;
    private const int rowsPerThread = _HEIGHT / ThreadCount;

    public NoiseTest () : base() {
        ClientSize = new(_WIDTH, _HEIGHT);
        Load += OnLoad;
    }

    private VertexArray quad;
    private Sampler2D tex;
    private Raster raster;
    private FastNoiseLite[] noises;
    private CountdownEvent countdown;
    private PassThrough passThrough;

    record struct Work (int ThreadIndex, float Millis, FastNoiseLite Noise, Raster Raster, CountdownEvent Countdown);

    private static void ProcArrays (Work work) {
        var (threadIndex, millis, noise, raster, countdown) = work;
        var start = rowsPerThread * threadIndex;
        var end = start + rowsPerThread;
        var offset = 4 * _WIDTH * start;
        for (var y = start; y < end; ++y) {
            var yscaled = _YSCALE * y;
            var yscaleddelayed = yscaled + millis;
            var yscaledshifted = yscaled + _YSCALE * _HEIGHT;
            for (var x = 0; x < _WIDTH; ++x, offset += 2) {
                var xscaled = _XSCALE * x;

                var b = .5f + .5f * noise.GetNoise(xscaled + millis, yscaled);
                var g = .5f + .5f * noise.GetNoise(xscaled + _XSCALE * _WIDTH, yscaleddelayed);
                var r = .5f + .5f * noise.GetNoise(xscaled, yscaledshifted);
                raster.Pixels[offset] /*  */ = (byte)((int)(127.5f * b + 127.5f) & 0xF0);
                raster.Pixels[++offset] /**/ = (byte)((int)(127.5f * g + 127.5f) & 0xF0);
                raster.Pixels[++offset] /**/ = (byte)((int)(127.5f * r + 127.5f) & 0xF0);
            }
        }
        var done = countdown.Signal();
    }

    void OnLoad (object sender, EventArgs _) {
        quad = new();
        passThrough = new();
        quad.Assign(new VertexBuffer<Vector4>(QuadVertices), passThrough.VertexPosition);
        tex = new(new(_WIDTH, _HEIGHT), TextureFormat.Rgba8) { Min = MinFilter.Nearest, Mag = MagFilter.Nearest, Wrap = Wrap.ClampToEdge };
        noises = new FastNoiseLite[ThreadCount];
        raster = new(ClientSize, 4, 1);
        raster.ClearU32(Color.Black);
        for (var i = 0; i < ThreadCount; ++i)
            noises[i] = new FastNoiseLite(123);
        countdown = new(ThreadCount);
        SetSwapInterval(1);
        Disable(Capability.DepthTest);
        StartThreads();
    }

    private static readonly Vector4[] QuadVertices = {
        new(-1f, -1f, 0, 1),
        new(+1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, +1f, 0, 1),
    };

    protected override void Render () {
        countdown.Wait();
        tex.Upload(raster);
        StartThreads();
        Viewport(new(), ClientSize);
        ClearColor(0f, 0f, 0f, 1f);
        Clear(BufferBit.ColorDepth);
        UseProgram(passThrough);
        BindVertexArray(quad);
        tex.BindTo(1);
        passThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);
    }

    private void StartThreads () {
        countdown.Reset(ThreadCount);
        var ms = FramesRendered / 3f;
        for (var i = 0; i < ThreadCount; ++i) {
            var ok = ThreadPool.QueueUserWorkItem(ProcArrays, new Work(i, ms, noises[i], raster, countdown), false);
            if (!ok)
                throw new ApplicationException();
        }
    }
}
