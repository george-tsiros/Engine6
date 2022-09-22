namespace Engine6;

using Win32;
using Gl;
using Shaders;
using System.Numerics;
using static Gl.GlContext;
using static Common.Maths;
using Common;

public class MatrixTests:GlWindow {

    public MatrixTests () :base(null, WindowStyle.OverlappedWindow) { 
        var m = Model.Cube(0.1f);
        Vertices = new Vector4[m.Faces.Count * 3];
        tempVertices = new Vector4[Vertices.Length];
        var vi = 0;
        foreach (var (i, j, k) in m.Faces) {
            Vertices[vi++] = new(m.Vertices[i] - Vector3.UnitZ, 1);
            Vertices[vi++] = new(m.Vertices[j] - Vector3.UnitZ, 1);
            Vertices[vi++] = new(m.Vertices[k] - Vector3.UnitZ, 1);
        }
    }

    private const float MouseSensitivity = .0005f;
    private const float KeyboardRotationSensitivity = .005f;
    private const float KeyboardTranslationSensitivity = .01f;

    private ProjectionOnly pProgram;
    private ViewProjection vpProgram;
    private VertexArray pVertexArray;
    private VertexArray vpVertexArray;
    private Vector3 origin;
    private VertexBuffer<Vector4> vb;
    private bool diy;
    private static readonly Ascii 
        Diy = new(nameof(Diy)), 
        OpenGL = new(nameof(OpenGL));
    private readonly Vector4[] 
        tempVertices,
        Vertices;
    protected override void OnKeyDown (in KeyArgs args) {
        switch (args.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
            case Key.Tab:
                diy = !diy;
                User32.SetWindowText(this, diy ? Diy : OpenGL);
                return;
        }
    }

    protected override void OnLoad () {
        vb = new(Vertices);

        UseProgram(vpProgram = new());
        BindVertexArray(vpVertexArray = new());
        vpVertexArray.Assign(vb, vpProgram.VertexPosition);

        UseProgram(pProgram = new());
        BindVertexArray(pVertexArray = new());
        pVertexArray.Assign(vb, pProgram.VertexPosition);

        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        ClearColor(0, 0, 0, 1);
        Disposables.Add(pProgram);
        Disposables.Add(pVertexArray);
        Disposables.Add(vpProgram);
        Disposables.Add(vpVertexArray);
        Disposables.Add(vb);
        Disposables.Add(Diy);
        Disposables.Add(OpenGL);
    }

    protected override void Render (double dt) {
        var size = ClientSize;
        Viewport(new(), size);
        Clear(BufferBit.ColorDepth);

        var xaxis = Axis(Key.C, Key.Z);
        var yaxis = Axis(Key.Q, Key.A);
        var zaxis = Axis(Key.D, Key.X);
        // timeSkew == 0 : time has stopped, nothing moves
        // 0 < timeSkew < 1 : framerate is too high, pretend as if less time has passed
        // timeSkew == 1 : exactly as planned
        // 1 < timeSkew : framerate is too low, pretend as if _more_ time has passed 
        var timeSkew = dt / TframeSeconds;
        var scale = KeyboardTranslationSensitivity * timeSkew;
        origin += (Vector3)(scale * new Vector3d(xaxis, yaxis, zaxis));
        
        var perspectiveProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, .1f, 100f);
        var translationMatrix = Matrix4x4.CreateTranslation(-origin);

        if (diy) {
            for (var i = 0; i < Vertices.Length; ++i)
                tempVertices[i] = Vector4.Transform(Vertices[i], translationMatrix);
            vb.BufferData(tempVertices, Vertices.Length, 0, 0);
            BindVertexArray(pVertexArray);
            UseProgram(pProgram);
            pProgram.Projection(perspectiveProjectionMatrix);
        } else {
            BindVertexArray(vpVertexArray);
            vb.BufferData(Vertices, Vertices.Length, 0, 0);
            UseProgram(vpProgram);
            vpProgram.View(translationMatrix);
            vpProgram.Projection(perspectiveProjectionMatrix);
        }
        DrawArrays(Primitive.Triangles, 0, 36);
    }

    private int Axis (Key plus, Key minus) {
        var x = IsKeyDown(plus) ? 1 : 0;
        var y = IsKeyDown(minus) ? -1 : 0;
        return x + y;
    }

}