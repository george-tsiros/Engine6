namespace Engine {
    using System;
    using System.Numerics;
    using Gl;
    using Shaders;
    using static Gl.Gl;
    using static Extra;
    using System.Diagnostics;

    class ShowTexture:GlWindowBase {
        public ShowTexture (GLFW.Monitor m) : base(m) { }
        public ShowTexture (int w, int h) : base(w, h) { }
        private uint _va, _t;
        const int __SIZE = 256;

        private float[] _pixels;
        private int _scale = 1;
        private OpenSimplex2S _noise;
        private float _sinceLastPress = float.MaxValue;
        protected override void Key (GLFW.Keys key, int code, GLFW.InputState state, GLFW.ModifierKeys modifier) {
            if (state == GLFW.InputState.Repeat)
                return;
            switch (key) {
                case GLFW.Keys.PageUp:
                    if (state == GLFW.InputState.Release && _scale < 8)
                        ResetTexture(_t, _pixels, ++_scale, __SIZE, _noise);
                    break;
                case GLFW.Keys.PageDown:
                    if (state == GLFW.InputState.Release && _scale > 0)
                        ResetTexture(_t, _pixels, --_scale, __SIZE, _noise);
                    break;
                case GLFW.Keys.Up:
                    _sinceLastPress = 0f;
                    break;
                case GLFW.Keys.Down:
                    break;
            }
        }
        private static void ResetTexture (uint texture, float[] pixels, int iscale, int size, OpenSimplex2S noise) {
            Debug.Assert(size * size == pixels.Length);
            Debug.Assert(0 <= iscale && iscale <= 8);
            var scale = Math.Pow(2, iscale) / size;
            for (var y = 0; y < size; ++y)
                for (var x = 0; x < size; ++x)
                    pixels[y * size + x] = (float)(noise.Noise2(scale * x, scale * y) * 0.5 + 0.5);
            using (Handle h = new(pixels))
                glTextureSubImage2D(texture, 0, 0, 0, size, size, GL.RED, GL.FLOAT, h);

        }
        protected override void Init () {
            _va = BindNewVertexArray();
            _noise = new OpenSimplex2S(0l);
            var textures = Create2DTextures(1);
#if !true
            _t = Texturing.FromRaw("untitled.raw");
#else
            _t = textures[0];
            glTextureStorage2D(_t, 1, GL.RGBA8, __SIZE, __SIZE);
            glTextureParameteri(_t, GL.TEXTURE_BASE_LEVEL, 0);
            glTextureParameteri(_t, GL.TEXTURE_MAX_LEVEL, 0);
            glTextureParameteri(_t, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            glTextureParameteri(_t, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            glTextureParameteri(_t, GL.TEXTURE_WRAP_S, GL.CLAMP_TO_EDGE);
            glTextureParameteri(_t, GL.TEXTURE_WRAP_T, GL.CLAMP_TO_EDGE);
            _pixels = new float[__SIZE * __SIZE];
            glActiveTexture(GL.TEXTURE0);
            glBindTexture(GL.TEXTURE_2D, _t);
            ResetTexture(_t, _pixels, _scale, __SIZE, _noise);
#endif
            glUseProgram(SimpleTexture.Id);
            _ = CreateBufferAndEnableAttribute(SimpleTexture.VertexPosition, Geometry.Quad);
            _ = CreateBufferAndEnableAttribute(SimpleTexture.VertexUV, Geometry.QuadUV);
            glUniform1i(SimpleTexture.Tex, 0);
            glUniformMatrix4fv(SimpleTexture.Model, 1, false, Matrix4x4.Identity);
            glUniformMatrix4fv(SimpleTexture.Projection, 1, false, Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 6), (float)Width / Height, 1f, 100f));
        }
        protected override void Render (float dt) {
            glViewport(0, 0, Width, Height);
            glClear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
            glDisable(GL.DEPTH_TEST);
            glDisable(GL.CULL_FACE);

            glBindVertexArray(_va);
            glActiveTexture(GL.TEXTURE0);
            glBindTexture(GL.TEXTURE_2D, _t);

            glUseProgram(SimpleTexture.Id);
            glUniformMatrix4fv(SimpleTexture.View, 1, false, Matrix4x4.CreateLookAt(4 * Vector3.UnitZ, Vector3.Zero, Vector3.UnitY));
            glDrawArrays(GL.TRIANGLES, 0, 6);
        }
    }
}
