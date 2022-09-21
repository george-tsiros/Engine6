namespace Engine6;

using Win32;
using Gl;
using Shaders;
using System.Numerics;
using static Gl.GlContext;
using static Common.Maths;

public class MatrixTests:GlWindow {

    private const float MouseSensitivity = .0005f;
    private const float KeyboardRotationSensitivity = .005f;
    private const float KeyboardTranslationSensitivity = .05f;

    private ViewProjection vpProgram;
    private ModelViewProjection mvpProgram;
    private VertexArray vpVertexArray;
    private VertexArray mvpVertexArray;
    private Vector3 origin;
    private Vector3 model;
    private VertexBuffer<Vector4> vb;
    private double theta = 0f;

    protected override void OnKeyDown (in KeyArgs args) {
        switch (args.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    protected override void OnLoad () {
        var m = Model.Cube(0.1f);
        var vertices = new Vector4[m.Faces.Count * 3];
        var vi = 0;
        foreach (var (i, j, k) in m.Faces) {
            vertices[vi++] = new(m.Vertices[i] - Vector3.UnitZ, 1);
            vertices[vi++] = new(m.Vertices[j] - Vector3.UnitZ, 1);
            vertices[vi++] = new(m.Vertices[k] - Vector3.UnitZ, 1);
        }
        vb = new(vertices);

        UseProgram(vpProgram = new());
        BindVertexArray(vpVertexArray = new());
        vpVertexArray.Assign(vb, vpProgram.VertexPosition);

        UseProgram(mvpProgram = new());
        BindVertexArray(mvpVertexArray = new());
        mvpVertexArray.Assign(vb, mvpProgram.VertexPosition);

        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        ClearColor(0, 0, 0, 1);
        Disposables.Add(mvpProgram);
        Disposables.Add(mvpVertexArray);
        Disposables.Add(vpProgram);
        Disposables.Add(vpVertexArray);
        Disposables.Add(vb);
    }

    protected override void Render (double dt) {
        var xaxis = Axis(Key.C, Key.Z);
        var yaxis = Axis(Key.D, Key.X);
        // timeSkew == 0 : time has stopped, nothing moves
        // 0 < timeSkew < 1 : framerate is too high, pretend as if less time has passed
        // timeSkew == 1 : exactly as planned
        // 1 < timeSkew : framerate is too low, pretend as if _more_ time has passed 
        var timeSkew = dt / TframeSeconds;
        var scale = KeyboardTranslationSensitivity * timeSkew;
        origin += new Vector3((float)(scale * xaxis), (float)(scale * yaxis), 0);
        theta += 0.1 * timeSkew / dTau;
        if (dTau < theta)
            theta %= dTau;
        var sin =(float) DoubleSin(theta);
        var size = ClientSize;
        Viewport(new(), size);
        Clear(BufferBit.ColorDepth);
        model = new(sin, sin, 0);
        BindVertexArray(vpVertexArray);
        UseProgram(vpProgram);
        vpProgram.View(Matrix4x4.CreateTranslation(-origin));
        vpProgram.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, .1f, 100f));
        DrawArrays(Primitive.Triangles, 0, 36);

        BindVertexArray(mvpVertexArray);
        UseProgram(mvpProgram);
        mvpProgram.Model(Matrix4x4.CreateTranslation(model));
        mvpProgram.View(Matrix4x4.CreateTranslation(-origin));
        mvpProgram.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, .1f, 100f));
        DrawArrays(Primitive.Triangles, 0, 36);
    }

    private int Axis (Key plus, Key minus) {
        var x = IsKeyDown(plus) ? 1 : 0;
        var y = IsKeyDown(minus) ? -1 : 0;
        return x + y;
    }

}
