namespace Engine6;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;

public sealed class Scene {

    public Scene Add (Model model, in Vector3 location) =>
        Add(model, location, Quaternion.Identity);

    public Scene Add (Model model, in Vector3 location, Quaternion orientation) {
        if (ready)
            throw new InvalidOperationException();
        models.Add(model);
        matrices.Add(Matrix4x4.CreateFromQuaternion(orientation)* Matrix4x4.CreateTranslation(location) );
        return this;
    }
    public ReadOnlySpan<Vector4> Vertices => ready ? vertices : throw new InvalidOperationException();
    public ReadOnlySpan<Vector4> Normals => ready ? normals : throw new InvalidOperationException();
    Vector4[] vertices, normals;
    public void Complete () {
        var vertexCount = 0;
        var normalCount = 0;
        foreach (var m in models) {
            vertexCount += m.Faces.Count * 3;
            normalCount += m.Normals.Count * 3;
            Debug.Assert(vertexCount == normalCount);
        }

        vertices = new Vector4[vertexCount];
        normals = new Vector4[normalCount];
        var (vi, ni) = (0, 0);
        for (var m = 0; m < models.Count; ++m){
            var model = models[m];
            var matrix = matrices[m];
            var v = new Vector3[model.Vertices.Count];

            for (var x = 0; x < v.Length; ++x) 
                v[x] = Vector3.Transform(model.Vertices[x], matrix);

            foreach (var (i, j, k) in model.Faces) {
                var (v0, v1, v2) = (v[i], v[j], v[k]);
                var n = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
                vertices[vi++] = new(v[i], 1);
                vertices[vi++] = new(v[j], 1);
                vertices[vi++] = new(v[k], 1);
                normals[ni++] = new(n, 0);
                normals[ni++] = new(n, 0);
                normals[ni++] = new(n, 0);
            }
        }
        ready = true;
    }
    private readonly List<Model> models = new();
    private readonly List<Matrix4x4> matrices = new();
    private bool ready;
}
