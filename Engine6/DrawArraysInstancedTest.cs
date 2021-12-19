//#define __USE_NEW__
namespace Engine {
    using System;
    using System.Numerics;
    using Gl;
    using Shaders;
    using static Gl.Gl;
    using static Extra;
    using System.Collections.Generic;
    using System.Diagnostics;

    class DrawArraysInstancedTest:GlWindowBase {
        //private static void Foo<T> (T[] eh) where T : struct {
        //}
        public DrawArraysInstancedTest (GLFW.Monitor m) : base(m) { }
        public DrawArraysInstancedTest (int w, int h) : base(w, h) { }
        private Vector2i _mousePosition;
        private Camera _camera = new(new(1, 1, 6));
        private uint _va;
        protected override void Key (GLFW.Keys key, int code, GLFW.InputState state, GLFW.ModifierKeys modifier) {
            //Foo(new int[] { });
            //Foo(new Vector3[] { });
            //Foo(new Matrix4x4[] { });


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
#if __USE_NEW__
#else
#endif
        private static IEnumerable<string> Constants (Type type, int value) {
            foreach (var fi in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
                if (fi.FieldType.Equals(typeof(int)) && fi.GetValue(null) is int ix && ix == value)
                    yield return fi.Name;
        }
        protected override void Init () {
            var ints = new int[10];

            glGetProgramInterfaceiv(SimpleTexture.Id, GL.PROGRAM_OUTPUT, GL.MAX_NAME_LENGTH, ints);
            var (actualLength, maxLength, size, type) = (0, 254, 0, 0);
            var buffer = new byte[255];
            glGetProgramiv(SimpleTexture.Id, GL.ACTIVE_UNIFORM_MAX_LENGTH, out maxLength);
            glGetProgramiv(SimpleTexture.Id, GL.ACTIVE_UNIFORMS, out var uniformCount);
            for (var i = 0u; i < uniformCount; ++i) {
                using (var h = new Handle(buffer))
                    glGetActiveUniform(SimpleTexture.Id, i, maxLength, out actualLength, out size, out type, h);
                var name = System.Text.Encoding.ASCII.GetString(buffer, 0, actualLength);
                var location = glGetUniformLocation(SimpleTexture.Id, name);
                Debug.WriteLine($"uniform {name} ({type}) {string.Join(", ", Constants(typeof(GL), type))}, size {size} at {location}");
            }

            glGetProgramiv(SimpleTexture.Id, GL.ACTIVE_ATTRIBUTE_MAX_LENGTH, out maxLength);
            glGetProgramiv(SimpleTexture.Id, GL.ACTIVE_ATTRIBUTES, out var attributeCount);
            for (var i = 0u; i < attributeCount; ++i) {
                using (var h = new Handle(buffer))
                    glGetActiveAttrib(SimpleTexture.Id, i, maxLength, out actualLength, out size, out type, h);
                var name = System.Text.Encoding.ASCII.GetString(buffer, 0, actualLength);
                var location = glGetAttribLocation(SimpleTexture.Id, name);
                Debug.WriteLine($"attrib {name} ({type}) {string.Join(", ", Constants(typeof(GL), type))}, size {size} at {location}");
            }


            GLFW.Glfw.GetCursorPosition(Window, out var mx, out var my);
            _mousePosition = new((int)Math.Floor(mx), (int)Math.Floor(my));
            var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 6), (float)Width / Height, 1f, 100f);
            _va = BindNewVertexArray();
#if __USE_NEW__
#else

            glUseProgram(DirectionalLightFlat.Id);
            glUniformMatrix4fv(DirectionalLightFlat.Projection, 1, false, projection);
            glUniform4f(DirectionalLightFlat.Color, 1, 1, 1, 1);
            glUniform4f(DirectionalLightFlat.LightDirection, -.25f, -.5f, -1, 1);
            _ = CreateBufferAndEnableAttribute(DirectionalLightFlat.Model, Array.ConvertAll(Geometry.CubeVertices, v => Matrix4x4.CreateTranslation(v.X * 2, v.Y * 2, v.Z * 2)), 1u);
            _ = CreateBufferAndEnableAttribute(DirectionalLightFlat.Vertex, Dex(TranslateInPlace(Geometry.CubeVertices, -.5f * Vector3.One), Geometry.CubeIndices));
            _ = CreateBufferAndEnableAttribute(DirectionalLightFlat.Normal, Dex(Geometry.CubeNormals, Geometry.CubeNormalIndices));
#endif
        }
        protected override void Render (float dt) {
            if (Focused && CursorGrabbed)
                _camera.Move(dt * 4f);
            glViewport(0, 0, Width, Height);
            glClear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
#if __USE_NEW__
#else
            glEnable(GL.DEPTH_TEST);
            glEnable(GL.CULL_FACE);
            glBindVertexArray(_va);
            glUseProgram(DirectionalLightFlat.Id);
            glUniformMatrix4fv(DirectionalLightFlat.View, 1, false, _camera.LookAtMatrix);
            glDrawArraysInstanced(GL.TRIANGLES, 0, 36, 8);
#endif
        }
    }
#if __USE_NEW__
#endif
}
