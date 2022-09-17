namespace Engine {
    using System;
    using System.Numerics;
    using Gl;
    using Shaders;
    using static Gl.Gl;
    using static Extra;
    class Ground:GlWindowBase {
        public Ground (GLFW.Monitor m) : base(m) { }
        public Ground (int w, int h) : base(w, h) { }
        private Vector2i _mousePosition;
        private uint _primaryVertexArray, _secondaryVertexArray, _iBuffer;
        private int _indicesTotal;
        private const int __SIZE = 64, __SCALE = 1;
        private Camera _camera = new(new(0, 25, __SCALE * __SIZE / 2f));
        private bool _useSecondary = true;
        protected override void Key (GLFW.Keys key, int code, GLFW.InputState state, GLFW.ModifierKeys modifier) {
            if (state == GLFW.InputState.Repeat)
                return;
            if (_camera.Key(key, state))
                return;
            switch (key) {
                case GLFW.Keys.Tab:
                    if (state == GLFW.InputState.Release)
                        CursorGrabbed = !CursorGrabbed;
                    break;
                case GLFW.Keys.F1:
                    if (state == GLFW.InputState.Release)
                        _useSecondary = !_useSecondary;
                    break;
            }
        }

        protected override void CursorPosition (IntPtr windowPtr, Vector2i v) {
            if (Focused && CursorGrabbed)
                _camera.Rotate(v - _mousePosition);
            _mousePosition = v;
        }

        protected override void Init () {
            GLFW.Glfw.GetCursorPosition(Window, out var mx, out var my);
            _mousePosition = new((int)Math.Floor(mx), (int)Math.Floor(my));
            _primaryVertexArray = BindNewVertexArray();
            var heightmap = new float[__SIZE * __SIZE];
            OpenSimplex2S n0 = new(2l);
            OpenSimplex2S n1 = new(1l);
            var o0 = 10.0 / __SIZE;
            var o1 = 2.0 / __SIZE;
            for (var z = 0; z < __SIZE; ++z)
                for (var x = 0; x < __SIZE; ++x)
                    heightmap[z * __SIZE + x] = (float)(n0.Noise2(o0 * x, o0 * z) + 1) + 10f * (float)(n1.Noise2(o1 * x, o1 * z) + 1);

            Vector3 lightDirection = new(1, 0.1f, 0);
            var lightColor = Vector3.One;
            var model = Matrix4x4.CreateTranslation(-__SIZE * .5f * __SCALE, 0f, -__SIZE * .5f * __SCALE);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 6), (float)Width / Height, 10f, 1000f);
            var quadsPerSide = __SIZE - 1;
            var quadsTotal = quadsPerSide * quadsPerSide;
            var trianglesTotal = quadsTotal * 2;
            var vertices = new Vector4[heightmap.Length];
            for (var (i, z) = (0, 0); z < __SIZE; ++z)
                for (var x = 0; x < __SIZE; ++x, ++i)
                    vertices[i] = new(x * __SCALE, heightmap[i], z * __SCALE, 1);
            _indicesTotal = trianglesTotal * 3;
            var indices = new int[_indicesTotal];
            for (var (i, z) = (0, 0); z < quadsPerSide; ++z)
                for (var x = 0; x < quadsPerSide; ++x) {
                    var ui = z * __SIZE + x;
                    indices[i++] = ui;
                    indices[i++] = ui + __SIZE;
                    indices[i++] = ui + 1 + __SIZE;
                    indices[i++] = ui + 1 + __SIZE;
                    indices[i++] = ui + 1;
                    indices[i++] = ui;
                }
            _iBuffer = CreateBuffers(1)[0];
            glBindBuffer(GL.ELEMENT_ARRAY_BUFFER, _iBuffer);
            BufferData(GL.ELEMENT_ARRAY_BUFFER, indices);

            glUseProgram(DirectionalLightFlat.Id);
            var vBuffer = CreateBufferAndEnableAttribute(DirectionalLightFlat.Vertex, vertices);
            var nBuffer = CreateBufferAndEnableAttribute(DirectionalLightFlat.Normal, Geometry.CreateNormals(vertices, indices));
            glUniform4f(DirectionalLightFlat.Color, lightColor.X, lightColor.Y, lightColor.Z, 1f);
            glUniform4f(DirectionalLightFlat.LightDirection, lightDirection.X, lightDirection.Y, lightDirection.Z, 1f);
            _ = CreateBufferAndEnableAttribute(DirectionalLightFlat.Model, new Matrix4x4[] { model }, 1);
            glUniformMatrix4fv(DirectionalLightFlat.Projection, 1, false, projection);

            _secondaryVertexArray = BindNewVertexArray();
            glUseProgram(PointLightFlat.Id);
            BindBufferAndEnableAttribute(PointLightFlat.Vertex, vBuffer, GL.FLOAT, 4);
            BindBufferAndEnableAttribute(PointLightFlat.Normal, nBuffer, GL.FLOAT, 4);
            glUniform4f(PointLightFlat.Color, lightColor.X, lightColor.Y, lightColor.Z, 1f);
            glUniformMatrix4fv(PointLightFlat.Model, 1, false, model);
            glUniformMatrix4fv(PointLightFlat.Projection, 1, false, projection);
        }
        protected override void Render (float dt) {
            if (Focused && CursorGrabbed)
                _camera.Move(dt * 4f);
            glViewport(0, 0, Width, Height);
            glClear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
            glEnable(GL.DEPTH_TEST);
            glEnable(GL.CULL_FACE);
            if (_useSecondary) {
                glBindVertexArray(_secondaryVertexArray);
                glUseProgram(PointLightFlat.Id);
                glUniformMatrix4fv(PointLightFlat.View, 1, false, _camera.LookAtMatrix);
                var ahead = _camera.Location + 4f * _camera.Ahead;
                glUniform4f(PointLightFlat.LightPosition, ahead.X, ahead.Y, ahead.Z, 1);
            } else {
                glBindVertexArray(_primaryVertexArray);
                glUseProgram(DirectionalLightFlat.Id);
                glUniformMatrix4fv(DirectionalLightFlat.View, 1, false, _camera.LookAtMatrix);
            }
            glBindBuffer(GL.ELEMENT_ARRAY_BUFFER, _iBuffer);
            glDrawElements(GL.TRIANGLES, _indicesTotal, GL.UNSIGNED_INT, IntPtr.Zero);
        }
    }
}

