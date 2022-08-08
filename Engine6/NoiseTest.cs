namespace Engine;

using System.Threading;
using Gl;
using static Gl.Opengl;
using Shaders;
using System.Numerics;
using System;
using System.Diagnostics;
using Win32;
using static Linear.Maths;
using Linear;

class NoiseTest:GlWindowArb {

    const int _WIDTH = 512, _HEIGHT = 512;
    const float _XSCALE = 1000f / _WIDTH, _YSCALE = 1000f / _HEIGHT;
    const int ThreadCount = 4;

    public NoiseTest (Vector2i size) : base(size) {
        rowsPerThread = _HEIGHT / ThreadCount;
        raster = new(size, 4, 1);
        Load += Load_self;
    }

    VertexArray quad;
    Sampler2D tex;
    Raster raster;
    //readonly byte[] bytes = new byte[_WIDTH * _HEIGHT * 4];
    readonly int rowsPerThread;
    FastNoiseLite[] noises;
    CountdownEvent countdown;

    void ProcArrays (int threadIndex) {
        var ms = FramesRendered;
        var start = rowsPerThread * threadIndex;
        var end = start + rowsPerThread;
        var offset = 4 * _WIDTH * start;
        var noise = noises[threadIndex];
        for (var y = start; y < end; ++y) {
            var yscaled = _YSCALE * y;
            var yscaleddelayed = yscaled + ms;
            var yscaledshifted = yscaled + _YSCALE * _HEIGHT;
            for (var x = 0; x < _WIDTH; ++x, offset += 2) {
                var xscaled = _XSCALE * x;

                var b = .5f + .5f * noise.GetNoise(xscaled + ms, yscaled);
                var g = .5f + .5f * noise.GetNoise(xscaled + _XSCALE * _WIDTH, yscaleddelayed);
                var r = .5f + .5f * noise.GetNoise(xscaled, yscaledshifted);
                raster.Pixels[offset] /*  */ = (byte)(127.5f * b + 127.5);
                raster.Pixels[++offset] /**/ = (byte)(127.5f * g + 127.5);
                raster.Pixels[++offset] /**/ = (byte)(127.5f * r + 127.5);
            }
        }
        var done = countdown.Signal();
    }

    void Load_self (object sender, EventArgs args) {
        quad = new();
        quad.Assign(new VertexBuffer<Vector4>(QuadVertices), PassThrough.VertexPosition);
        tex = new(new(_WIDTH, _HEIGHT), TextureFormat.Rgba8) { Min = MinFilter.Nearest, Mag = MagFilter.Nearest, Wrap = Wrap.ClampToEdge };
        noises = new FastNoiseLite[ThreadCount];
        raster.ClearU32(Color.Black);
        for (var i = 0; i < ThreadCount; ++i)
            noises[i] = new FastNoiseLite(123);
        countdown = new(ThreadCount);
        StartThreads();
    }

    static readonly Vector4[] QuadVertices = {
        new(-1f, -1f, 0, 1),
        new(+1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, +1f, 0, 1),
    };

    unsafe protected override void Render () {
        countdown.Wait();
        countdown.Reset(ThreadCount);
        tex.Upload(raster);
        Viewport(0, 0, Width, Height);
        ClearColor(0f, 0f, 0f, 1f);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        tex.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);
        StartThreads();
    }

    void StartThreads () {
        for (var i = 0; i < ThreadCount; ++i) {
            var ok = ThreadPool.QueueUserWorkItem(ProcArrays, i, true);
            if (!ok)
                throw new ApplicationException();
        }
    }
}
