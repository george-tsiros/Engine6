namespace Engine6;

using System.Windows.Forms;
using System;
using Win32;
using static Gl.Opengl;
using Gl;
using Shaders;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Collections.Generic;

public class GlForm:Form {
    const double T = 1e7;
    public GlForm () {
        Debug.Assert(T == Stopwatch.Frequency);
        foreach (var style in Enum.GetValues<ControlStyles>())
            Debug.WriteLine($"{style}: {GetStyle(style)}");
        SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
        Dc = new(Handle);

        //var config = new ContextConfigurationARB { BasicConfiguration = new() { DoubleBuffer = true }, Profile=ProfileMask.Core };
        //ctx = CreateContextARB(Dc, config);
        ctx = CreateContextARB(Dc, new() { BasicConfiguration = new() { ColorBits = 32, DepthBits = 32, DoubleBuffer = true }, Flags = ContextFlag.Debug | ContextFlag.ForwardCompatible, Profile = ProfileMask.Core });
        va = new();
        p = new SolidColor();
        UseProgram(p);
        va.Assign(new VertexBuffer<Vector4>(Quad), p.VertexPosition, 0);
        p.Color(new(1, .5f, 1, 1));
        p.Model(Matrix4x4.Identity);
        p.Projection(Matrix4x4.Identity);
        p.View(Matrix4x4.Identity);
        Enable(Capability.CullFace);
        Disable(Capability.DepthTest);
        Closing += Closing_self;
    }

    void Closing_self (object sender, CancelEventArgs args) {
        va.Dispose();
        p.Dispose();
        ReleaseCurrent(Dc);
        ctx = IntPtr.Zero;
        Dc.Close();
        Dc = null;
    }

    DeviceContext Dc;
    IntPtr ctx;
    static readonly Vector4[] Quad = {
        new(-1, -1, 0, 1),
        new(1, -1, 0, 1),
        new(1, 1, 0, 1),
        new(-1, -1, 0, 1),
        new(1, 1, 0, 1),
        new(-1, 1, 0, 1),
    };

    VertexArray va;
    SolidColor p;
    protected override void OnKeyDown (KeyEventArgs args) {
        switch (args.KeyCode) {
            case Keys.Escape:
                Close();
                return;
        }
        base.OnKeyDown(args);
    }

    protected override void OnKeyUp (KeyEventArgs args) { }
    static double Mean (double[] x) {
        var sum = 0.0;
        for (var i = x.Length; 0 <= --i;)
            sum += x[i];
        return sum / x.Length;
    }
    private readonly double[] q = new double[10];
    int qi;
    long lastSync;
    bool ready;
    static double TicksToSeconds (long t) => t / T;
    protected override void OnPaint (PaintEventArgs e) {
        Viewport(0, 0, ClientSize.Width, ClientSize.Height);
        ClearColor(0, 0.5f, 0, 1);
        Clear(BufferBit.ColorDepth);
        DrawArrays(Primitive.Triangles, 0, 6);

        Gdi32.SwapBuffers(Dc);
        var now = Stopwatch.GetTimestamp();
        if (0 < lastSync) {
            q[qi] = TicksToSeconds(now - lastSync);
            if (10 == ++qi) {
                ready = true;
                qi = 0;
            }
        }
        lastSync = now;
        Invalidate();
        if (ready)
            Text = (1.0 / Mean(q)).ToString();
    }
}
