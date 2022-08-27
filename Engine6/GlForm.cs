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

public class GlForm:Form {


    public GlForm (ContextConfiguration? configuration =null) {
        foreach (var style in Enum.GetValues<ControlStyles>())
            Debug.WriteLine($"{style}: {GetStyle(style)}");
        SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
        Dc = new(Handle);
        ctx = CreateSimpleContext(Dc, configuration);
        MakeCurrent((IntPtr)Dc, ctx);
        va = new();
        p = new SolidColor();
        va.Assign(new VertexBuffer<Vector4>(Quad), p.VertexPosition, 0);
        p.Color(new(1, 1, 1, 1));
        p.Model(Matrix4x4.Identity);
        p.Projection(Matrix4x4.Identity);
        p.View(Matrix4x4.Identity);
        Closing += Closing_self;
    }

    void Closing_self (object sender, CancelEventArgs args) {
        va.Dispose();
        p.Dispose();
        ReleaseCurrent((IntPtr)Dc);
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

    protected override void OnKeyUp (KeyEventArgs args) {

    }

    protected override void OnPaint (PaintEventArgs e) {
        Viewport(0, 0, ClientSize.Width, ClientSize.Height);
        ClearColor(0, 0.5f, 0, 1);
        Clear(BufferBit.ColorDepth);
        UseProgram(p);
        State.VertexArrayBinding = va;
        DrawArrays(Primitive.Triangles, 0, 6);
        var dc = e.Graphics.GetHdc();
        Gdi32.SwapBuffers(dc);
        e.Graphics.ReleaseHdc(dc);
        Invalidate();
    }
}
