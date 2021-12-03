namespace Engine {
    using System;
    using System.Numerics;
    using Gl;
    using Shaders;
    using static Gl.Gl;
    using static Extra;

    class SkyBoxTest:GlWindowBase {
        public SkyBoxTest (GLFW.Monitor m) : base(m) { }
        public SkyBoxTest (int w, int h) : base(w, h) { }
        private Vector2i _mousePosition;
        private Camera _camera = new(new(1, 1, 6));
        private uint _cube, _skyVertexArray, _skyTexture;

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
            var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 6), (float)Width / Height, 1f, 100f);
            var model = Array.ConvertAll(Geometry.CubeVertices, v => Matrix4x4.CreateTranslation(2 * v.X, 2 * v.Y, 2 * v.Z));
            _skyTexture = Texturing.FromRaw("skybox.raw");
            var flippedVertexIndices = Geometry.CubeIndices;
            Geometry.FlipWinding(flippedVertexIndices);
            var verdexIndicesDexed = Dex(TranslateInPlace(Geometry.CubeVertices, -.5f * Vector3.One), flippedVertexIndices);
            var uvFlipped = Geometry.CubeUVIndices;
            Geometry.FlipWinding(uvFlipped);
            var uvFlippedDexed = Dex(Geometry.CubeUVVectors, uvFlipped);

            _cube = BindNewVertexArray();

            glUseProgram(DirectionalLightFlat.Id);
            glUniformMatrix4fv(DirectionalLightFlat.Projection, 1, false, projection);
            glUniform4f(DirectionalLightFlat.Color, 1, 1, 1, 1);
            glUniform4f(DirectionalLightFlat.LightDirection, -.25f, -.5f, -1, 1);
            _ = CreateBufferAndEnableAttribute(DirectionalLightFlat.Vertex, Dex(TranslateInPlace(Geometry.CubeVertices, -.5f * Vector3.One), Geometry.CubeIndices));
            _ = CreateBufferAndEnableAttribute(DirectionalLightFlat.Normal, Dex(Geometry.CubeNormals, Geometry.CubeNormalIndices));
            _ = CreateBufferAndEnableAttribute(DirectionalLightFlat.Model, model, 1);

            glUseProgram(SkyBox.Id);
            glUniform1i(SkyBox.Tex, 0);
            glUniformMatrix4fv(SkyBox.Projection, 1, false, projection);

            _skyVertexArray = BindNewVertexArray();
            _ = CreateBufferAndEnableAttribute(SkyBox.VertexPosition, verdexIndicesDexed);
            _ = CreateBufferAndEnableAttribute(SkyBox.VertexUV, uvFlippedDexed);
        }
        protected override void Render (float dt) {
            if (Focused && CursorGrabbed)
                _camera.Move(dt * 4f);
            glViewport(0, 0, Width, Height);
            glClear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
            glEnable(GL.DEPTH_TEST);
            glEnable(GL.CULL_FACE);

            glDepthFunc(GL.LESS);
            glBindVertexArray(_cube);

            glUseProgram(DirectionalLightFlat.Id);
            glUniformMatrix4fv(DirectionalLightFlat.View, 1, false, _camera.LookAtMatrix);
            glDrawArraysInstanced(GL.TRIANGLES, 0, 36, 8);

            glDepthFunc(GL.LEQUAL);
            glBindVertexArray(_skyVertexArray);
            glActiveTexture(GL.TEXTURE0);
            glBindTexture(GL.TEXTURE_2D, _skyTexture);

            glUseProgram(SkyBox.Id);
            glUniformMatrix4fv(SkyBox.View, 1, false, _camera.RotationOnly);

            glDrawArrays(GL.TRIANGLES, 0, 36);
        }
    }
}